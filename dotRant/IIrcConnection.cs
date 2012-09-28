using System;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IIrcConnection
    {
        Task Connect(IConnectionFactory connectionFactory = null);
        IChannelList Channels { get; }
        string Nick { get; set; }

        event EventHandler<CommandEventArgs> RawMessageIn;
        event EventHandler<CommandEventArgs> RawMessageOut;
        event EventHandler<ChannelEventArgs> Join;
        event EventHandler<ChannelNameEventArgs> Part;
        event EventHandler<ChannelTopicEventArgs> ChannelTopicChanged;

        event EventHandler<ExceptionEventArgs> UnhandledException;

        Task SendRawCommand(string text);
    }
}
