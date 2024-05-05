// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
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

        /// <inheritdoc />
        public override ILogger LoggerImpl => new Logger(new CompositeLogHandler(loggers));

        /// <inheritdoc />
        public override void Dispose()
        {
            // Nothing to dispose.
        }

        private class CompositeLogHandler : ILogHandler
        {
            private readonly List<AbstractLogger> _loggers;

            public CompositeLogHandler(List<AbstractLogger> loggers)
            {
                _loggers = loggers;
            }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                foreach (var logger in _loggers)
                {
                    logger.LoggerImpl.LogFormat(logType, context, format, args);
                }
            }

            public void LogException(Exception exception, Object context)
            {
                foreach (var logger in _loggers)
                {
                    logger.LoggerImpl.LogException(exception, context);
                }
            }
        }
    }
}
