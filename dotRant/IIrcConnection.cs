using System;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcConnection
    {
        Task Connect(IConnectionFactory connectionFactory = null);

        event EventHandler<CommandEventArgs> RawMessageIn;
        event EventHandler<CommandEventArgs> RawMessageOut;
    }
}
