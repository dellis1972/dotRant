using dotRant.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class ExceptionEventArgs : EventArgs
    {
        private IrcException _exception;

        public ExceptionEventArgs(IrcException exception)
        {
            _exception = exception;
        }

        public IrcException Exception
        {
            get { return _exception; }
        }
    }
}
