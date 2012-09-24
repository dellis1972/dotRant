
namespace dotRant.Net45
{
    class NLogLogger : ILogger
    {
        readonly NLog.Logger _logger;

        public NLogLogger(NLog.Logger logger)
        {
            _logger = logger;
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
