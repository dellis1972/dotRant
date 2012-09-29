using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcUser : IIrcTarget, IComparable<IIrcUser>
    {
    }

    public interface IIrcChannelUser : IIrcUser, IComparable<IIrcChannelUser>
    {
        IIrcChannel Channel { get; }
    }
}
