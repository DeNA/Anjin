// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Utilities
{
    /// <summary>
    /// Logger using <c>Debug.unityLogger</c>
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly ILogger _loggerImpl = Debug.unityLogger;

        /// <inheritdoc />
        public ILogHandler logHandler { get; set; }

        /// <inheritdoc />
        public bool logEnabled { get; set; }

        /// <inheritdoc />
        public LogType filterLogType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handler"></param>
        public ConsoleLogger(ILogHandler handler)
        {
            this.logHandler = handler;
            this.logEnabled = true;
            this.filterLogType = LogType.Log;
        }

        /// <inheritdoc />
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            _loggerImpl.LogFormat(logType, context, format, args);
        }

        /// <inheritdoc />
        public void LogException(Exception exception, Object context)
        {
            _loggerImpl.LogException(exception, context);
        }

        /// <inheritdoc />
        public bool IsLogTypeAllowed(LogType logType)
        {
            return _loggerImpl.IsLogTypeAllowed(logType);
        }

        /// <inheritdoc />
        public void Log(LogType logType, object message)
        {
            _loggerImpl.Log(logType, message);
        }

        /// <inheritdoc />
        public void Log(LogType logType, object message, Object context)
        {
            _loggerImpl.Log(logType, message, context);
        }

        /// <inheritdoc />
        public void Log(LogType logType, string tag, object message)
        {
            _loggerImpl.Log(logType, tag, message);
        }

        /// <inheritdoc />
        public void Log(LogType logType, string tag, object message, Object context)
        {
            _loggerImpl.Log(logType, tag, message, context);
        }

        /// <inheritdoc />
        public void Log(object message)
        {
            _loggerImpl.Log(message);
        }

        /// <inheritdoc />
        public void Log(string tag, object message)
        {
            _loggerImpl.Log(tag, message);
        }

        /// <inheritdoc />
        public void Log(string tag, object message, Object context)
        {
            _loggerImpl.Log(tag, message, context);
        }

        /// <inheritdoc />
        public void LogWarning(string tag, object message)
        {
            _loggerImpl.LogWarning(tag, message);
        }

        /// <inheritdoc />
        public void LogWarning(string tag, object message, Object context)
        {
            _loggerImpl.LogWarning(tag, message, context);
        }

        /// <inheritdoc />
        public void LogError(string tag, object message)
        {
            _loggerImpl.LogError(tag, message);
        }

        /// <inheritdoc />
        public void LogError(string tag, object message, Object context)
        {
            _loggerImpl.LogError(tag, message, context);
        }

        /// <inheritdoc />
        public void LogFormat(LogType logType, string format, params object[] args)
        {
            _loggerImpl.LogFormat(logType, format, args);
        }

        /// <inheritdoc />
        public void LogException(Exception exception)
        {
            _loggerImpl.LogException(exception);
        }
    }
}
