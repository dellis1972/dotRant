using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace dotRant.TestWpfClient
{
    abstract class BufferViewModel : BaseViewModel
    {
        private ObservableCollection<IrcEntry> _entries;

        public BufferViewModel(Dispatcher dispatcher)
            : base(dispatcher)
        {
            _entries = new ObservableCollection<IrcEntry>();
        }

        public abstract string Name { get; }
        public ObservableCollection<IrcEntry> Entries
        {
            get { return _entries; }
        }

        public abstract void SendTo(string message);
    }
}
