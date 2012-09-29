using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcChannelUserList : IEnumerable<IIrcChannelUser>, IEnumerable
    {
        int Count { get; }

        IIrcChannelUser this[int index] { get; }
        IIrcChannelUser this[string nick] { get; }
    }
}
