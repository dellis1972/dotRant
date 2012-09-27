using System;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcConnection
    {
        Task Connect(IConnectionFactory connectionFactory = null);
        IChannelList Channels { get; }

        event EventHandler<CommandEventArgs> RawMessageIn;
        event EventHandler<CommandEventArgs> RawMessageOut;
        event EventHandler<ChannelEventArgs> Join;

        event EventHandler<ExceptionEventArgs> UnhandledException;

        Task SendRawCommand(string text);
    }
}
