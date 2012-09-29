using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    class IrcChannel : IIrcChannel
    {
        internal enum State
        {
            WaitingForJoin,
            WaitingForNames,
            Joined,
            Parting
        }

        internal enum UserMode
        {
            None,
            HalfVoice,
            Voice,
            HalfOp,
            Op,
            HalfAdmin,
            Admin
        }

        internal readonly IrcConnection _connection;
        internal readonly ChannelUserList _userList;
        readonly string _name;
        internal readonly List<Guid> _users;
        internal readonly Dictionary<Guid, UserMode> _userStates;
        internal readonly Dictionary<Guid, IrcChannelUser> _userObjects;

        internal volatile TaskCompletionSource<IIrcChannel> _loaded;
        internal volatile State _state;

        internal string _topic = "";
        internal string _topicCreator = null;
        internal DateTime _topicTime = DateTime.MinValue;

        public IrcChannel(IrcConnection connection, string name)
        {
            _connection = connection;
            _name = name;
            _users = new List<Guid>();
            _userStates = new Dictionary<Guid, UserMode>();
            _loaded = new TaskCompletionSource<IIrcChannel>();
            _state = State.WaitingForJoin;
            _userList = new ChannelUserList(this);
            _userObjects = new Dictionary<Guid, IrcChannelUser>();
        }

        public string Name
        {
            get { return _name; }
        }

        public string Topic
        {
            get { return _topic; }
            set
            {
                if (_topic != value)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public string TopicCreator
        {
            get { return _topicCreator; }
        }

        public DateTime TopicTime
        {
            get { return _topicTime; }
        }

        public Task Part() 
        {
            return _connection.PartChannel(_name);
        }

        public IIrcConnection Connection
        {
            get { return _connection; }
        }

        public IIrcChannelUserList Users
        {
            get
            {
                return _userList;
            }
        }

        public Task Send(string message)
        {
            return _connection.SendCommand("PRIVMSG", _name, message.AsMultiParameter());
        }

        internal void AddName(string name)
        {
            var prefixes = ParsePrefixes(ref name);
            Guid nameGuid = _connection.Name(name);
            _users.Add(nameGuid);
            _userStates.Add(nameGuid, ModeFromPrefixes(prefixes));
        }

        internal IrcChannelUser RemoveName(string name)
        {
            var user = _userList[name];
            _users.Remove(user._guid);
            _userStates.Remove(user._guid);
            _userObjects.Remove(user._guid);
            return user;
        }

        private string ParsePrefixes(ref string name)
        {
            StringBuilder prefixes = new StringBuilder();
            while (name[0] == '+' || name[0] == '@' || name[0] == '!')
            {
                prefixes.Append(name[0]);
                name = name.Substring(1);
            }
            return prefixes.ToString();
        }

        private UserMode ModeFromPrefixes(string prefixes)
        {
            switch (prefixes)
            {
                case "@": return UserMode.Op;
                case "!": return UserMode.Admin;
                case "+": return UserMode.Voice;
            }
            return UserMode.None;
        }
    }
}
