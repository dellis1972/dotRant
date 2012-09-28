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

        internal Task PartChannel(string name)
        {
            lock (_channels)
            {
                var channel = GetChannel(name);
                channel._state = IrcChannel.State.Parting;
                SendCommand("PART", name);

                channel._loaded = new TaskCompletionSource<IIrcChannel>();
                return channel._loaded.Task;
            }
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
                    else
                    {
                        channel = new IrcChannel(this, args[0]);
                        _channels.Add(args[0], channel);
                        channel._state = IrcChannel.State.WaitingForNames;
                        return;
                    }
                }
            }
        }

        [IrcCommand("PART")]
        async Task HandlePart(string prefix, string command, string[] args)
        {
            var client = ParseClient(prefix);
            if (client._nick == _nick)
            {
                lock (_channels)
                {
                    IrcChannel channel;
                    if (_channels.TryGetValue(args[0], out channel))
                    {
                        if (channel._state == IrcChannel.State.Parting)
                        {
                            channel._loaded.SetResult(null);
                        }

                        _channels.Remove(channel.Name);
                        OnPart(channel.Name);
                        return;
                    }
                    throw new InvalidOperationException(Resources.PartedNonJoinedChannel);
                }
            }
        }


        [IrcCommand("332")]
        async Task HandleJoinTopic(string prefix, string command, string[] args)
        {
            //332 <nick> <channel> :<topic>
            if (args[0] == _nick)
            {
                lock (_channels)
                {
                    string channelName = args[1];
                    IrcChannel channel;

                    if(_channels.TryGetValue(channelName, out channel))
                    {
                        channel._topic = args[2];
                        return;
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        [IrcCommand("333")]
        async Task HandleJoinTopicTime(string prefix, string command, string[] args)
        {
            //333 <channel> <topic creator nick> <topic created time>
            if (args[0] == _nick)
            {
                lock (_channels)
                {
                    string channelName = args[1];
                    IrcChannel channel;

                    if (_channels.TryGetValue(channelName, out channel))
                    {
                        channel._topicTime = Utils.UnixTimeStampToDateTime(double.Parse(args[3]));
                        channel._topicCreator = args[2];
                        return;
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        [IrcCommand("TOPIC")]
        async Task HandleTopic(string prefix, string command, string[] args)
        {
            //TOPIC <channel> :<topic>
            lock (_channels)
            {
                var client = ParseClient(prefix);

                string channelName = args[0];
                IrcChannel channel;

                if (_channels.TryGetValue(channelName, out channel))
                {
                    string oldTopic = channel._topic;

                    channel._topic = args[1];
                    channel._topicCreator = client._nick;
                    channel._topicTime = DateTime.Now;
                    OnChannelTopicChanged(channel, oldTopic);
                    return;
                }
                throw new InvalidOperationException();
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
                                channel._state = IrcChannel.State.Joined;
                                channel._loaded.SetResult(channel);
                                OnJoin(channel);
                                return;
                            }
                        }
                    }
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
