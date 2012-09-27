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

        Task Part();
    }
}
