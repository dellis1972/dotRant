using System;
using System.IO;
using System.Threading.Tasks;

namespace dotRant
{
	public class StreamWrapper : IDisposable
	{
		public Stream InputStream { get; set; }
		public Stream OutputStream { get; set; }

		public void Dispose ()
		{
			if (InputStream != null) {
				InputStream.Dispose ();
				InputStream = null;
			}
			try {
				if (OutputStream != null) {
					OutputStream.Dispose ();
					OutputStream = null;
				}
			} catch {
			}
		}
	}
 
    public interface IConnectionFactory
    {
        Task<StreamWrapper> Connect(string hostname, int port, bool useSsl);
    }
}
