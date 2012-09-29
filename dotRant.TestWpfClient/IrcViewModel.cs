using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace dotRant.TestWpfClient
{
    class IrcViewModel : BaseViewModel
    {
        readonly IIrcConnection _conn;
        readonly ObservableCollection<BufferViewModel> _buffers;

        public IrcViewModel(Dispatcher dispatcher, IIrcConnection conn)
            : base(dispatcher)
        {
            _conn = conn;
            _buffers = new ObservableCollection<BufferViewModel>();

            foreach (var chann in _conn.Channels)
            {
                _buffers.Add(new ChannelViewModel(_dispatcher, chann));
            }

            _conn.NickChanged += _conn_NickChanged;
            _conn.Join += _conn_Join;
            _conn.Part += _conn_Part;
            _conn.Message += _conn_Message;
            _conn.Channels.UserJoined += _conn_Channels_UserJoined;
            _conn.Channels.UserParted += _conn_Channels_UserParted;
        }

        void _conn_Channels_UserParted(object sender, ChannelUserEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)delegate
            {
                var buff = (ChannelViewModel)_buffers.SingleOrDefault(b => b.Name == e.Channel.Name);
                buff.Users.Remove(e.User);
            });
        }

        void _conn_Channels_UserJoined(object sender, ChannelUserEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)delegate
            {
                var buff = (ChannelViewModel)_buffers.SingleOrDefault(b => b.Name == e.Channel.Name);
                IIrcChannelUser[] users = new IIrcChannelUser[buff.Users.Count + 1];
                buff.Users.CopyTo(users, 0);
                users[users.Length - 1] = e.User;
                Array.Sort(users);
                buff.Users.Insert(users.ToList().IndexOf(e.User), e.User);
            });
        }

        void _conn_Message(object sender, MessageEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)delegate
            {
                var buff = _buffers.SingleOrDefault(b => b.Name == e.Receiver.Name);
                if (buff == null)
                {
                    return; // not implemented
                    //throw new NotImplementedException();
                }
                buff.Entries.Add(new IrcEntry(e.Sender + ": " + e.Message));
            });
        }

        void _conn_Part(object sender, ChannelNameEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)delegate
            {
                var buff = _buffers.Single(b => b.Name == e.ChannelName);
                _buffers.Remove(buff);
            });
        }

        void _conn_Join(object sender, ChannelEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)delegate
            {
                var index = _conn.Channels.ToList().IndexOf(e.Channel);
                _buffers.Insert(index, new ChannelViewModel(_dispatcher, e.Channel));
            });
        }

        void _conn_NickChanged(object sender, NickEventArgs e)
        {
            if (e.NewNick == _conn.Nick)
                DispatchPropertyChanged(() => Nick);
        }

        public string Nick
        {
            get { return _conn.Nick; }
        }

        public ObservableCollection<BufferViewModel> Buffers
        {
            get { return _buffers; }
        }
    }
}
