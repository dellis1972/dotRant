using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IChannelList : IEnumerable<IIrcChannel>
    {
        Task<IIrcChannel> Join(string name, string password = null);
        IIrcChannel this[int index] { get; }
        IIrcChannel this[string name] { get; }
        int Count { get; }
    }
}
