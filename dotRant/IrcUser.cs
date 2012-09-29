using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    class IrcUser : IIrcUser
    {
        internal readonly IrcConnection _connection;
        internal readonly Guid _guid;

        public IrcUser(IrcConnection connection, Guid guid)
        {
            _connection = connection;
            _guid = guid;
        }

        public IIrcConnection Connection
        {
            get { return _connection; }
        }

        public Task Send(string message)
        {
            return _connection.SendCommand("PRIVMSG", Name, message);
        }

        public string Name
        {
            get { return _connection.Name(_guid); }
        }

        public int CompareTo(IIrcUser other)
        {
            return Name.CompareTo(other.Name);
        }
    }

    class IrcChannelUser : IrcUser, IIrcChannelUser
    {
        internal readonly IrcChannel _channel;

        public IrcChannelUser(Guid guid, IrcChannel channel)
            : base(channel._connection, guid)
        {
            // TODO: Complete member initialization
            _channel = channel;
        }

        public IIrcChannel Channel
        {
            get { return _channel; }
        }

        public int CompareTo(IIrcChannelUser other)
        {
            var channelCompare = Channel.Name.CompareTo(other.Channel.Name);
            if (channelCompare == 0)
                return ((IrcUser)this).CompareTo(other);
            return channelCompare;
        }
    }
}
