using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcChannel : IIrcTarget
    {
        IList<string> Users { get; }
        string Topic { get; set; }
        string TopicCreator { get; }
        DateTime TopicTime { get; }

        Task Part();
    }
}
