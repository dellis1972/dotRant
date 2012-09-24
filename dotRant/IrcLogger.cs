
namespace dotRant
{
    class IrcLogger : ILogger
    {
        readonly ILogger _logger;
        readonly IrcConnection _connection;

        public IrcLogger(IrcConnection connection, ILogger logger)
        {
            _logger = logger;
            _connection = connection;
        }

        public void Trace(string message, params object[] args)
        {
            _logger.Trace(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            _logger.Fatal(message, args);
        }
    }
}
