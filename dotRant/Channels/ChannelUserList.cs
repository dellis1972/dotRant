using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    class ChannelUserList : IIrcChannelUserList
    {
        readonly IrcChannel _channel;

        public ChannelUserList(IrcChannel channel)
        {
            _channel = channel;
        }

        public int Count
        {
            get { return _channel._users.Count; }
        }

        internal IrcChannelUser this[int index]
        {
            get
            {
                var guid = _channel._users[index];
                return this[guid];
            }
        }

        internal IrcChannelUser this[string nick]
        {
            get
            {
                var guid = _channel._connection.Name(nick, false);
                return this[guid];
            }
        }

        private IrcChannelUser this[Guid guid]
        {
            get
            {
                IrcChannelUser user;
                if (!_channel._userObjects.TryGetValue(guid, out user))
                {
                    _channel._userObjects[guid] = user = new IrcChannelUser(guid, _channel);
                }
                return user;
            }
        }

        IIrcChannelUser IIrcChannelUserList.this[int index]
        {
            get { return this[index]; }
        }

        IIrcChannelUser IIrcChannelUserList.this[string nick]
        {
            get { return this[nick]; }
        }

        IEnumerator<IIrcChannelUser> IEnumerable<IIrcChannelUser>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IIrcChannelUser>)this).GetEnumerator();
        }
    }
}
