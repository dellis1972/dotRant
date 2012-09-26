using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    partial class IrcConnection
    {
        readonly Dictionary<string, IrcChannel> _channels = new Dictionary<string, IrcChannel>();
        readonly Dictionary<string, Guid> _nicks = new Dictionary<string, Guid>();
        readonly Dictionary<Guid, string> _nicksRev = new Dictionary<Guid, string>();

        internal Task<IIrcChannel> JoinChannel(string name, string password)
        {
            if (name[0] != '#' && name[0] != '+')
            {
                name = '#' + name;
            }

            IrcChannel channel;
            lock (_channels)
            {
                if (GetChannel(name) != null)
                {
                    throw new InvalidOperationException("Channel already joined");
                }
                channel = new IrcChannel(this, name);
                _channels.Add(name, channel);
            }

            if (password != null)
                SendCommand("JOIN", name, password);
            else
                SendCommand("JOIN", name);

            return channel._loaded.Task;
        }

        internal IrcChannel GetChannel(string name)
        {
            lock (_channels)
            {
                IrcChannel c;
                if (_channels.TryGetValue(name, out c))
                {
                    if (c._state == IrcChannel.State.Joined)
                        return c;
                }
                return null;
            }
        }

        internal Guid Name(string nick)
        {
            lock (_nicks)
            {
                Guid ret;
                if (!_nicks.TryGetValue(nick, out ret))
                {
                    ret = Guid.NewGuid();
                    _nicks.Add(nick, ret);
                    _nicksRev.Add(ret, nick);
                }
                return ret;
            }
        }

        internal string Name(Guid guid)
        {
            lock(_nicks)
                return _nicksRev[guid];
        }

        [IrcCommand("JOIN")]
        async Task HandleJoin(string prefix, string command, string[] args)
        {
            var client = ParseClient(prefix);
            if (client._nick == _nick) // we just joined a channel. Wait for names (do nothing).
            {
                lock (_channels)
                {
                    IrcChannel channel;
                    if (_channels.TryGetValue(args[0], out channel))
                    {
                        if (channel._state == IrcChannel.State.WaitingForJoin)
                        {
                            channel._state = IrcChannel.State.WaitingForNames;
                            return;
                        }
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        [IrcCommand("353")]
        [IrcCommand("366")]
        async Task HandleNameList(string prefix, string command, string[] args)
        {
            if (args[0] == _nick)
            {
                lock (_channels)
                {
                    string channelName = command == "353" ? args[2] : args[1];
                    IrcChannel channel;
                    if (_channels.TryGetValue(channelName, out channel))
                    {
                        if (channel._state == IrcChannel.State.WaitingForNames)
                        {
                            if (command == "353") // names
                            {
                                // :<server> 353 <nick> = <channel> :<names>
                                foreach (var name in args[3].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    channel.AddName(name);
                                }
                                return;
                            }
                            else if(command == "366")
                            {
                                // :<server> 366 <nick> <channel> :End of /NAMES list.
                                channel._loaded.SetResult(channel);
                            }
                        }
                    }
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
