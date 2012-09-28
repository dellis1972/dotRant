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

        IEnumerator<IIrcChannel> IEnumerable<IIrcChannel>.GetEnumerator()
        {
            return _connection._channels.OrderBy(c => c.Key).Select(c => c.Value).Cast<IIrcChannel>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IIrcChannel>)this).GetEnumerator();
        }
    }
}
