using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant
{
    [AttributeUsage(AttributeTargets.Method)]
    class IrcCommandAttribute : Attribute
    {
        string _command;
        public IrcCommandAttribute(string command)
        {
            _command = command;
        }

        public string Command
        {
            get { return _command; }
        }
    }
}
