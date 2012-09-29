using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class MessageEventArgs : EventArgs
    {
        private string _message;
        private IIrcTarget _receiver;
        private string _sender;

        public MessageEventArgs(string message, IIrcTarget receiver, string sender)
        {
            _message = message;
            _receiver = receiver;
            _sender = sender;
        }

        public string Message
        {
            get { return _message; }
        }

        public IIrcTarget Receiver
        {
            get { return _receiver; }
        }

        public string Sender
        {
            get { return _sender; }
        }
    }
}
