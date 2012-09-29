using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class ChannelNameEventArgs : EventArgs
    {
        private string _channelName;
        public ChannelNameEventArgs(string channelName)
        {
            _channelName = channelName;
        }

        public string ChannelName
        {
            get { return _channelName; }
        }
    }
}
