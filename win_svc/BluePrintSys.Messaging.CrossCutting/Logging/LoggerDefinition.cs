using System;
using NServiceBus.Logging;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public class LoggerDefinition : LoggingFactoryDefinition
    {
        LogLevel level = LogLevel.Info;

        public void Level(LogLevel level)
        {
            this.level = level;
        }

        protected override ILoggerFactory GetLoggingFactory()
        {
            return new LoggerFactory(level);
        }
    }

    class LoggerFactory : ILoggerFactory
    {
        LogLevel level;

        public LoggerFactory(LogLevel level)
        {
            this.level = level;
        }

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new LogAdapter(name, level);
        }
    }

    class LogAdapter : ILog
    {
        string name;
        public bool IsDebugEnabled { get; }
        public bool IsInfoEnabled { get; }
        public bool IsWarnEnabled { get; }
        public bool IsErrorEnabled { get; }
        public bool IsFatalEnabled { get; }

        public LogAdapter(string name, LogLevel level)
        {
            this.name = name;
            IsDebugEnabled = LogLevel.Debug >= level;
            IsInfoEnabled = LogLevel.Info >= level;
            IsWarnEnabled = LogLevel.Warn >= level;
            IsErrorEnabled = LogLevel.Error >= level;
            IsFatalEnabled = LogLevel.Fatal >= level;
        }

        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            Log.Debug($"{message}\n{exception}");
        }

        public void DebugFormat(string format, params object[] args)
        {
            Log.DebugFormat(format, args);
        }

        public void Info(string message)
        {
            Log.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            Log.Info($"{message}\n{exception}");
        }

        public void InfoFormat(string format, params object[] args)
        {
            Log.InfoFormat(format, args);
        }

        public void Warn(string message)
        {
            Log.Warn(message);
        }

        public void Warn(string message, Exception exception)
        {
            Log.Warn($"{message}\n{exception}");
        }

        public void WarnFormat(string format, params object[] args)
        {
            Log.WarnFormat(format, args);
        }

        public void Error(string message)
        {
            Log.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            Log.Error($"{message}\n{exception}");
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Log.ErrorFormat(format, args);
        }

        public void Fatal(string message)
        {
            Log.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            Log.Fatal($"{message}\n{exception}");
        }

        public void FatalFormat(string format, params object[] args)
        {
            Log.FatalFormat(format, args);
        }
    }
}
