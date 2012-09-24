using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace dotRant.Net45
{
    public class IrcConnectionFactory : dotRant.IrcConnectionFactory, dotRant.IConnectionFactory, dotRant.ILoggerFactory
    {
        public IrcConnectionFactory()
            : base()
        {

        }

        protected override IConnectionFactory ConnectionFactory
        {
            get { return this; }
        }

        protected override ILoggerFactory LoggerFactory
        {
            get { return this; }
        }

        async Task<Stream> IConnectionFactory.Connect(string hostname, int port, bool useSsl)
        {
            TcpClient client = new TcpClient();
            client.NoDelay = true;
            await client.ConnectAsync(hostname, port);
            if (useSsl)
            {
                SslStream sslStream = new SslStream(client.GetStream());
                await sslStream.AuthenticateAsClientAsync(hostname);
                return sslStream;
            }
            return client.GetStream();
        }

        ILogger ILoggerFactory.GetLogger(Type type)
        {
            if (type.IsAbstract || !type.IsPublic)
                return new NLogLogger(NLog.LogManager.GetLogger(type.FullName));
            return new NLogLogger(NLog.LogManager.GetLogger(type.FullName, type));
        }

        ILogger ILoggerFactory.GetLogger(string name)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(name));
        }

        ILogger ILoggerFactory.GetLogger(object instance)
        {
            return ((ILoggerFactory)this).GetLogger(instance.GetType());
        }
    }
}
