using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace dotRant.TestWpfClient
{
    class ChannelViewModel : BufferViewModel
    {
        IIrcChannel _channel;
        readonly ObservableCollection<IIrcChannelUser> _users;

        public ChannelViewModel(Dispatcher dispatcher, IIrcChannel channel)
            : base(dispatcher)
        {
            _channel = channel;
            _users = new ObservableCollection<IIrcChannelUser>();
            foreach (var u in channel.Users.OrderBy(un => un.Name))
                _users.Add(u);
        }

        public override string Name
        {
            get { return _channel.Name; } 
        }

        public override ObservableCollection<IIrcChannelUser> Users
        {
            get { return _users; }
        }

        public string Topic
        {
            get { return _channel.Topic; }
            set { _channel.Topic = value; }
        }

        internal void TopicUpdated()
        {
            DispatchPropertyChanged(() => Topic);
        }

        public override async void SendTo(string message)
        {
            await _channel.Send(message);
            _dispatcher.BeginInvoke((Action)delegate
            {
                Entries.Add(new IrcEntry(_channel.Connection.Nick + ": " + message));
            });
        }
    }
}
