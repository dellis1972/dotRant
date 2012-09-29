using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    class ChannelList : IChannelList
    {
        readonly IrcConnection _connection;

        public ChannelList(IrcConnection connection)
        {
            _connection = connection;
        }

        public Task<IIrcChannel> Join(string name, string password = null)
        {
            return _connection.JoinChannel(name, password);
        }

        public IIrcChannel this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public IIrcChannel this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        #region IEnumerable

        IEnumerator<IIrcChannel> IEnumerable<IIrcChannel>.GetEnumerator()
        {
            return _connection._channels.OrderBy(c => c.Key).Select(c => c.Value).Cast<IIrcChannel>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IIrcChannel>)this).GetEnumerator();
        }

        #endregion

        #region Events

        public event EventHandler<ChannelTopicEventArgs> TopicChanged;
        public event EventHandler<ChannelUserEventArgs> UserJoined;
        public event EventHandler<ChannelUserEventArgs> UserParted;

        internal void OnTopicChanged(IrcChannel channel, string oldTopic)
        {
            if (TopicChanged != null)
                TopicChanged(_connection, new ChannelTopicEventArgs(channel, oldTopic));
        }

        internal void OnUserJoined(IrcChannel channel, IrcChannelUser user)
        {
            if (UserJoined != null)
                UserJoined(_connection, new ChannelUserEventArgs(channel, user));
        }

        internal void OnUserParted(IrcChannel channel, IrcChannelUser user)
        {
            if (UserParted != null)
                UserParted(_connection, new ChannelUserEventArgs(channel, user));
        }

        #endregion
    }
}
