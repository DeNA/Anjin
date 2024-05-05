// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using DeNA.Anjin.Loggers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.TestDoubles
{
    public class SpyLogger : AbstractLogger
    {
        private SpyLogHandler _handler;
        private ILogger _logger;

        public override ILogger LoggerImpl
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

        public List<string> Logs => _handler.Logs;

        private class SpyLogHandler : ILogHandler
        {
            public readonly List<string> Logs = new List<string>();

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                Logs.Add(string.Format(format, args));
            }

            public void LogException(Exception exception, Object context)
            {
                Logs.Add(exception.ToString());
            }
        }
    }
}
