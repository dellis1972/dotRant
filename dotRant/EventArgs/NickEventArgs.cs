using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    public class NickEventArgs : EventArgs
    {
        private string _oldNick;
        private string _newNick;

        public NickEventArgs(string oldNick, string newNick)
        {
            _oldNick = oldNick;
            _newNick = newNick;
        }

        public string OldNick
        {
            get { return _oldNick; }
        }

        public string NewNick
        {
            get { return _newNick; }
        }
    }
}
