// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// A class for a logger that delegates to multiple loggers
    /// </summary>
    [CreateAssetMenu(fileName = "New CompositeLogger", menuName = "Anjin/Composite Logger", order = 70)]
    public class CompositeLogger : AbstractLogger
    {
        /// <summary>
        /// Loggers to delegates
        /// </summary>
        public List<AbstractLogger> loggers = new List<AbstractLogger>();

        private CompositeLogHandler _handler;
        private Logger _logger;

        /// <inheritdoc />
        public override ILogger LoggerImpl
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                _handler = new CompositeLogHandler(loggers, this);
                _logger = new Logger(_handler);
                return _logger;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _handler?.Dispose();
        }

        private class CompositeLogHandler : ILogHandler, IDisposable
        {
            private readonly List<AbstractLogger> _loggers;
            private readonly AbstractLogger _owner;

            public CompositeLogHandler(List<AbstractLogger> loggers, AbstractLogger owner)
            {
                _loggers = loggers;
                _owner = owner;
            }

            /// <inheritdoc />
            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                foreach (var logger in _loggers.Where(logger => logger != null && logger != _owner))
                {
                    logger.LoggerImpl.LogFormat(logType, context, format, args);
                }
            }

            /// <inheritdoc />
            public void LogException(Exception exception, Object context)
            {
                foreach (var logger in _loggers.Where(logger => logger != null && logger != _owner))
                {
                    logger.LoggerImpl.LogException(exception, context);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                foreach (var logger in _loggers.Where(logger => logger != null && logger != _owner))
                {
                    logger.Dispose();
                }
            }
        }
    }
}
