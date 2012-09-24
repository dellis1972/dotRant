
namespace dotRant
{
    public abstract class IrcConnectionFactory
    {
        private ILogger _logger;

        public IrcConnectionFactory()
        {
            _logger = LoggerFactory.GetLogger(typeof(IrcConnectionFactory));
        }

        protected abstract IConnectionFactory ConnectionFactory { get; }
        protected abstract ILoggerFactory LoggerFactory { get; }

        public virtual IIrcConnection Create(string hostname, bool useSsl = false, int port = 6667, string password = null)
        {
            _logger.Debug("Connecing to {0}:{1} using ssl: {2}", hostname, port, useSsl);
            IrcConnection connection = new IrcConnection(LoggerFactory, ConnectionFactory, hostname, useSsl, port, password);
            return connection;
        }
    }
}
