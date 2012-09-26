using System;
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
    }
}
