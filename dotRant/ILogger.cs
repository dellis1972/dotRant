using System;

namespace dotRant
{
    public interface ILogger
    {
        void Trace(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Info(string message, params object[] args);
        void Warn(string message, params object[] args);
        void Fatal(string message, params object[] args);
        //void Trace(string message, params object[] args);
    }

    public interface ILoggerFactory
    {
        ILogger GetLogger(Type type);
        ILogger GetLogger(string name);
        ILogger GetLogger(object instance);
    }
}
