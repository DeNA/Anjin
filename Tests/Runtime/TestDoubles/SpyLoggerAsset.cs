// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using DeNA.Anjin.Loggers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.TestDoubles
{
    public class SpyLoggerAsset : AbstractLoggerAsset
    {
        public bool Disposed { get; private set; }

        private SpyLogHandler _handler;
        private ILogger _logger;

        public override ILogger Logger
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                _handler = new SpyLogHandler();
                _logger = new Logger(_handler);
                return _logger;
            }
        }

        public List<(LogType logType, string message)> Logs => _handler.Logs;

        public override void Dispose()
        {
            Disposed = true;
        }

        private class SpyLogHandler : ILogHandler
        {
            public readonly List<(LogType logType, string message)> Logs = new List<(LogType, string)>();

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                Logs.Add((logType, string.Format(format, args)));
            }

            public void LogException(Exception exception, Object context)
            {
                Logs.Add((LogType.Exception, exception.ToString()));
            }
        }
    }
}
