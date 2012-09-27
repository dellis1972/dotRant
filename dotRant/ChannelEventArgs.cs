using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class ChannelEventArgs : EventArgs
    {
        private IIrcChannel _channel;
        public ChannelEventArgs(IIrcChannel channel)
        {
            _channel = channel;
        }

        public IIrcChannel Channel
        {
            get { return _channel; }
        }
    }
}
