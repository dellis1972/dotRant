using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    class IrcUser : IIrcUser
    {
        public IIrcConnection Connection
        {
            get { throw new NotImplementedException(); }
        }

        public Task Send(string message)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
