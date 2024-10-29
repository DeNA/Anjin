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
    /// A class for a logger that delegates to multiple loggers.
    /// This class is for Internal use. However, It is public for historical reasons.
    /// </summary>
    public class CompositeLoggerAsset : AbstractLoggerAsset
    {
        /// <summary>
        /// Loggers to delegate.
        /// </summary>
        public List<AbstractLoggerAsset> loggerAssets = new List<AbstractLoggerAsset>();

        private CompositeLogHandler _handler;
        private Logger _logger;

        /// <inheritdoc />
        public override ILogger Logger
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                _handler = new CompositeLogHandler(loggerAssets, this);
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
            private readonly List<AbstractLoggerAsset> _loggerAssets;
            private readonly AbstractLoggerAsset _owner;

            public CompositeLogHandler(List<AbstractLoggerAsset> loggerAssets, AbstractLoggerAsset owner)
            {
                _loggerAssets = loggerAssets;
                _owner = owner;
            }

            /// <inheritdoc />
            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                foreach (var loggerAsset in _loggerAssets.Where(x => x != null && x != _owner))
                {
                    loggerAsset.Logger.LogFormat(logType, context, format, args);
                }
            }

            /// <inheritdoc />
            public void LogException(Exception exception, Object context)
            {
                foreach (var loggerAsset in _loggerAssets.Where(x => x != null && x != _owner))
                {
                    loggerAsset.Logger.LogException(exception, context);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                foreach (var loggerAsset in _loggerAssets.Where(x => x != null && x != _owner))
                {
                    loggerAsset.Dispose();
                }
            }
        }
    }
}
