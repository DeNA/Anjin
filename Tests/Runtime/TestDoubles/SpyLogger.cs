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
        public List<string> Logs => ((SpyLogHandler)((Logger)LoggerImpl).logHandler).Logs;

        public SpyLogger()
        {
            LoggerImpl = new Logger(new SpyLogHandler());
        }

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
