using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class ChannelUserEventArgs : ChannelEventArgs
    {
        readonly IIrcChannelUser _user;

        public ChannelUserEventArgs(IIrcChannel channel, IIrcChannelUser user)
            : base(channel)
        {
            _user = user;
        }

        public IIrcChannelUser User
        {
            get { return _user; }
        }
    }
}
