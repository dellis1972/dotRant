using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotRant.TestWpfClient
{
    class IrcEntry
    {
        private string _content;

        public IrcEntry(string content)
        {
            _content = content;
        }

        public string Content
        {
            get { return _content; }
        }
    }
}
