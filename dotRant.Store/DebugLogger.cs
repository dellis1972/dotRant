using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant.Store
{
	public class DebugLogger : ILogger
	{
		public void Trace (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (String.Format ("Trace : {0}", string.Format(message, args)));
		}

		public void Debug (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (String.Format ("Debug : {0}", string.Format (message, args)));
		}

		public void Info (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (String.Format ("Info : {0}", string.Format (message, args)));
		}

		public void Warn (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (String.Format ("Warn : {0}", string.Format (message, args)));
		}

		public void Fatal (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (String.Format ("Fatal : {0}", string.Format (message, args)));
		}
	}
}
