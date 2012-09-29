using System;

namespace dotRant
{
    public class CommandEventArgs : EventArgs
    {
        private string _command;

        public CommandEventArgs(string command)
        {
            _command = command;
        }

        public string Command
        {
            get { return _command; }
        }
    }
}
