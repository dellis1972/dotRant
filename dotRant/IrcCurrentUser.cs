using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    class IrcCurrentUser : IIrcUser
    {
        private IrcConnection _conn;

        public IrcCurrentUser(IrcConnection conn)
        {
            _conn = conn;
        }

        public IIrcConnection Connection
        {
            get { return _conn; }
        }

        public Task Send(string message)
        {
            return _conn.SendCommand("PRIVMSG", Name, message);
        }

        public string Name
        {
            get { return _conn.Nick; }
        }
    }
}
