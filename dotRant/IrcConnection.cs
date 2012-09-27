using dotRant.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace dotRant
{
    partial class IrcConnection : IIrcConnection, IIrcIdentity
    {
        const string VERSION = "dotRant 0.1";
        const string CRLF = "\r\n";

        internal enum State
        {
            Offline,
            Connecting,
            Connected
        }

        internal struct Parameter
        {
            readonly string _value;
            readonly bool _forceMulti;

            public Parameter(string value, bool forceMulti = false)
            {
                _value = value;
                _forceMulti = forceMulti;
            }

            public override string ToString()
            {
                return (MultiArg ? ":" : "") + _value;
            }

            public bool MultiArg
            {
                get { return _forceMulti || _value.Contains(" "); }
            }

            public static implicit operator Parameter(string val)
            {
                return new Parameter(val);
            }
        }

        internal struct OutCommand
        {
            internal readonly TaskCompletionSource<bool> _tcs;
            internal readonly string _value;

            public OutCommand(TaskCompletionSource<bool> tcs, string value)
            {
                _tcs = tcs;
                _value = value;
            }
        }

        internal struct Command
        {
            internal readonly string _prefix;
            internal readonly string _command;
            internal readonly IList<string> _parameters;

            public Command(string prefix, string command, string[] parameters)
            {
                _prefix = prefix;
                _command = command;
                _parameters = parameters;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (_prefix != null)
                    sb.Append(':').Append(_prefix).Append(' ');

                sb.Append(_command);
                foreach (var p in _parameters)
                    sb.Append(p.Contains(" ") ? ":" : "").Append(p);

                return sb.ToString();
            }
        }

        internal struct Client
        {
            internal readonly string _nick;
            internal readonly string _login;
            internal readonly string _address;

            public Client(string nick, string login, string address)
            {
                _nick = nick;
                _login = login;
                _address = address;
            }
        }

        static int botCount = 0;

        readonly int _botCount;
        string _hostname;
        string _nick = "dotRant", _login = "dotRant";
        int _port;
        string _password;
        bool _useSsl;
        Stream _stream;
        StreamReader _reader;
        StreamWriter _writer;
        ILogger _logger;
        IConnectionFactory _connFactory;

        readonly ChannelList _channelList;

        static CancellationToken _outCancelToken = new CancellationToken(true);
        static readonly object _outLock = new object();
        static readonly Queue<Tuple<IrcConnection, OutCommand>> _outQueue = new Queue<Tuple<IrcConnection, OutCommand>>();
        static readonly AutoResetEvent _outWaiter = new AutoResetEvent(false);

        volatile State _state;

        public IrcConnection(ILoggerFactory loggerFactory, IConnectionFactory connectionFactory, string hostname, bool useSsl = true, int port = 6667, string password = null)
        {
            _botCount = Interlocked.Increment(ref botCount);

            _hostname = hostname;
            _port = port;
            _password = password;
            _useSsl = useSsl;
            _state = State.Offline;
            _logger = new IrcLogger(this, loggerFactory.GetLogger(this));
            _connFactory = connectionFactory;
            _channelList = new ChannelList(this);
        }

        public event EventHandler<CommandEventArgs> RawMessageIn;
        public event EventHandler<CommandEventArgs> RawMessageOut;
        public event EventHandler<ExceptionEventArgs> UnhandledException;
        public event EventHandler<ChannelEventArgs> Join;
        public event EventHandler<ChannelEventArgs> Part;
        public event EventHandler<ChannelTopicEventArgs> ChannelTopicChanged;

        private void OnRawMessageIn(string message)
        {
            if (RawMessageIn != null)
                RawMessageIn(this, new CommandEventArgs(message));
        }

        private void OnRawMessageOut(string message)
        {
            if (RawMessageOut != null)
                RawMessageOut(this, new CommandEventArgs(message));
        }

        private void OnUnhandledException(IrcException exception)
        {
            if (UnhandledException != null)
                UnhandledException(this, new ExceptionEventArgs(exception));
        }

        private void OnJoin(IrcChannel channel)
        {
            if (Join != null)
                Join(this, new ChannelEventArgs(channel));
        }

        private void OnChannelTopicChanged(IrcChannel channel, string oldTopic)
        {
            if (ChannelTopicChanged != null)
                ChannelTopicChanged(this, new ChannelTopicEventArgs(channel, oldTopic));
        }

        private void OnPart(IrcChannel channel)
        {
            if (Part != null)
                Part(this, new ChannelEventArgs(channel));
        }

        public IChannelList Channels
        {
            get { return _channelList; }
        }

        public string Nick
        {
            get { return _nick; }
            set
            {
                if (_nick != value)
                {
                    lock (this)
                    {
                        State state = _state;
                        if (state == State.Connecting)
                            throw new InvalidOperationException(Resources.NickChangeWhileConnecting);
                        if (state == State.Offline)
                            _nick = value;
                        else if (state == State.Connected)
                            SendCommand("NICK", value);
                    }
                }
            }
        }

        string IIrcIdentity.Login
        {
            get { return _login; }
        }

        public async Task Connect(IConnectionFactory connectionFactory = null)
        {
            if (connectionFactory == null)
                connectionFactory = _connFactory;
            await Connect(connectionFactory, this);
            return;
        }

        private void ClearAll()
        {
            _nicks.Clear();
            _nicksRev.Clear();
            _channels.Clear();
        }

        internal async Task Connect(IConnectionFactory connectionFactory, IIrcIdentity identity)
        {
            lock (this)
            {
                State state = _state;
                if (state == State.Connected)
                    throw new InvalidOperationException("Already connected");
                else if (state == State.Connecting)
                    throw new InvalidOperationException("Already connecting");

                _state = State.Connecting;
            }

            ClearAll();
            _logger.Debug("Initializing connection");
            _stream = await connectionFactory.Connect(_hostname, _port, _useSsl);

            if (_stream == null)
                throw new IrcException(Resources.ConnectionIsNull);
            _logger.Debug("Connected to server");

            var encoding = new UTF8Encoding(false);
            _reader = new StreamReader(_stream, encoding);
            _writer = new StreamWriter(_stream, encoding);
            _writer.NewLine = CRLF;

            StartOutboundThread();
            if (_password != null)
                await SendCommand("PASS", _password);

            await SendCommand("NICK", identity.Nick);
            await SendCommand("USER", identity.Login, "8", "*", VERSION.AsMultiParameter());
            _nick = identity.Nick;
            _login = identity.Login;

            //OnRawMessageIn(await _reader.ReadLineAsync());
            //await Task.Delay(TimeSpan.FromSeconds(10));

            // Read stuff back from the server to see if we connected.
            string line;
            int tries = 0;
            Command cmd = default(Command);

            while ((line = await _reader.ReadLineAsync()) != null)
            {
                OnRawMessageIn(line);
                cmd = ParseCmd(line);
                switch (cmd._command)
                {
                    //Check for both a successful connection. Inital connection (001-4), user stats (251-5), or MOTD (375-6)
                    case "001":
                    case "002":
                    case "003":
                    case "004":
                    case "005":
                    case "251":
                    case "252":
                    case "253":
                    case "254":
                    case "255":
                    case "375":
                    case "376":
                        goto end;

                    case "433":
                        var tmpNick = identity.Nick + (++tries);
                        await SendCommand("NICK", tmpNick);
                        _nick = tmpNick;
                        break;

                    case "439":
                        //EXAMPLE: PircBotX: Target change too fast. Please wait 104 seconds
                        // No action required.
                        break;

                    default:
                        if (cmd._command.StartsWith("5") || cmd._command.EndsWith("4"))
                        {
                            _stream.Dispose();
                            throw new IrcException(String.Format(Resources.CouldNotConnect, cmd));
                        }
                        break;
                }
            }

            _stream.Dispose();
            throw new IrcException(String.Format(Resources.CouldNotConnect, cmd));

        end:
            _logger.Debug("Logged in");

            lock (this)
                _state = State.Connected;

            if (cmd._parameters != null)
                await HandleCmd(cmd);

            StartInboundThread();

        }

        private Command ParseCmd(string cmd)
        {
            string prefix = null, name = null;
            List<string> args = new List<string>();

            if (cmd[0] == ':')
            {
                prefix = cmd.Substring(1, cmd.IndexOf(' ') - 1);
                cmd = cmd.Substring(cmd.IndexOf(' ') + 1);
            }

            var nameEnd = cmd.IndexOf(' ');
            name = cmd.Substring(0, nameEnd);
            cmd = cmd.Substring(nameEnd + 1);

            while (cmd.Length > 0)
            {
                if (cmd[0] == ':')
                {
                    args.Add(cmd.Substring(1));
                    cmd = "";
                }
                else
                {
                    var paramEnd = cmd.IndexOf(' ');
                    if (paramEnd == -1)
                        paramEnd = cmd.Length;
                    var param = cmd.Substring(0, paramEnd);
                    args.Add(param);
                    if (paramEnd != cmd.Length)
                        cmd = cmd.Substring(paramEnd + 1);
                    else
                        cmd = "";
                }
            }

            return new Command(prefix, name, args.ToArray());
        }

        private Client ParseClient(string clientString)
        {
            int pos = clientString.IndexOf('!');
            string nick = clientString.Substring(0, pos);
            clientString = clientString.Substring(pos + 1);
            pos = clientString.IndexOf('@');
            string login = clientString.Substring(0, pos);
            clientString = clientString.Substring(pos + 1);
            return new Client(nick, login, clientString);
        }

        private void StartOutboundThread()
        {
            lock (_outLock)
            {
                if (_outCancelToken.IsCancellationRequested)
                {
                    _outCancelToken = new CancellationToken();
                    Task.Factory.StartNew(() =>
                    {
                        Dictionary<IrcConnection, Task> waiters = new Dictionary<IrcConnection, Task>();
                        while (true)
                        {
                            _outWaiter.WaitOne(TimeSpan.FromSeconds(5));
                            _outCancelToken.ThrowIfCancellationRequested();

                            Queue<Tuple<IrcConnection, OutCommand>> queue = new Queue<Tuple<IrcConnection, OutCommand>>();
                            lock (_outLock)
                            {
                                while (_outQueue.Count > 0)
                                    queue.Enqueue(_outQueue.Dequeue());
                            }

                            while (queue.Count > 0)
                            {
                                var entry = queue.Dequeue();

                                if (waiters.ContainsKey(entry.Item1))
                                {
                                    var task = waiters[entry.Item1];
                                    if (task.IsCompleted)
                                        waiters[entry.Item1] = SendCommand(entry.Item2);
                                    else
                                        waiters[entry.Item1] = waiters[entry.Item1].ContinueWith(t => SendCommand(entry.Item2)).Unwrap();
                                }
                                else
                                {
                                    waiters[entry.Item1] = SendCommand(entry.Item2);
                                }
                            }

                        }

                    }, _outCancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
            }
        }

        private void StartInboundThread()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var line = await _reader.ReadLineAsync();
                    OnRawMessageIn(line);
                    var cmd = ParseCmd(line);
                    await HandleCmd(cmd);
                }
            });
        }

        private async Task SendCommand(OutCommand cmd)
        {
            await _writer.WriteLineAsync(cmd._value);
            await _writer.FlushAsync();
            OnRawMessageOut(cmd._value);
            cmd._tcs.SetResult(true);
        }

        internal Task SendCommand(string str, params Parameter[] args)
        {
            StringBuilder sb = new StringBuilder(str);
            for (var i = 0; i < args.Length; i++)
            {
                sb.Append(' ').Append(args[i]);
            }

            return SendRawCommand(sb.ToString());
        }

        public Task SendRawCommand(string rawCommand)
        {
            var tcs = new TaskCompletionSource<bool>();
            lock (_outLock)
                _outQueue.Enqueue(new Tuple<IrcConnection, OutCommand>(this, new OutCommand(tcs, rawCommand)));
            _outWaiter.Set();

            return tcs.Task;
        }

        private async Task HandleCmd(Command cmd)
        {
            try
            {
                await HandleCommand(this, cmd._prefix, cmd._command, cmd._parameters.ToArray());
            }
            catch (IrcException e)
            {
                OnUnhandledException(e);
            }
            catch (Exception e)
            {
                OnUnhandledException(new IrcException("An error occured during handling of message " + cmd.ToString(), e));
            }
        }
    }

    static class ParameterExtensions
    {
        public static IrcConnection.Parameter AsMultiParameter(this string s)
        {
            return new IrcConnection.Parameter(s, true);
        }
    }
}
