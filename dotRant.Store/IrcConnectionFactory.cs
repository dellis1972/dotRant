using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;

namespace dotRant.Store
{
	public class IrcConnectionFactory : dotRant.IrcConnectionFactory, dotRant.IConnectionFactory, dotRant.ILoggerFactory
	{
		public IrcConnectionFactory ()
			: base ()
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

		async Task<StreamWrapper> IConnectionFactory.Connect (string hostname, int port, bool useSsl)
		{
			var socket = new StreamSocket ();
			socket.Control.KeepAlive = true;
			socket.Control.NoDelay = true;
			await socket.ConnectAsync (new HostName (hostname), port.ToString(), useSsl ? SocketProtectionLevel.Ssl : SocketProtectionLevel.PlainSocket);
			return new StreamWrapper () { OutputStream = socket.OutputStream.AsStreamForWrite (), InputStream = socket.InputStream.AsStreamForRead () };
		}

		static readonly ILogger logger = new DebugLogger ();

		ILogger ILoggerFactory.GetLogger (Type type)
		{
			return logger;
		}

		ILogger ILoggerFactory.GetLogger (string name)
		{
			return logger;
		}

		ILogger ILoggerFactory.GetLogger (object instance)
		{
			return logger;
		}
	}

	
}
