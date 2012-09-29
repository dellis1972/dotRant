using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class ChannelTopicEventArgs : ChannelEventArgs
    {
        readonly string _oldTopic;
        public ChannelTopicEventArgs(IIrcChannel channel, string oldTopic)
            : base(channel)
        {
            _oldTopic = oldTopic;
        }

        public string OldTopic
        {
            get { return _oldTopic; }
        }
    }
}
