using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcTarget
    {
        IIrcConnection Connection { get; }
        Task Send(string message);
        string Name { get; }
    }
}
