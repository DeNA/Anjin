// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// Logger that output to a file.
    /// </summary>
    /// <remarks>
    /// If you want more functionality in the future, consider using the Unity Logging package (com.unity.logging) or ZLogger.
    /// <see href="https://docs.unity3d.com/Packages/com.unity.logging@latest"/>
    /// <see href="https://github.com/Cysharp/ZLogger"/>
    /// </remarks>
    [CreateAssetMenu(fileName = "New FileLogger", menuName = "Anjin/File Logger", order = 72)]
    public class FileLogger : AbstractLogger
    {
        /// <summary>
        /// Log output file path.
        /// Note: relative path from the project root directory. When run on player, it will be the Application.persistentDataPath.
        /// </summary>
        public string outputPath;

        /// <summary>
        /// To selective enable debug log message.
        /// </summary>
        public LogType filterLogType = LogType.Log;

        private FileLogHandler _handler;
        private ILogger _logger;

        /// <inheritdoc />
        public override ILogger LoggerImpl
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                var directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _handler = new FileLogHandler(outputPath);
                _logger = new Logger(_handler) { filterLogType = filterLogType };
                return _logger;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _handler?.Dispose();
        }

        private class FileLogHandler : ILogHandler, IDisposable
        {
            private readonly StreamWriter _writer;

            public FileLogHandler(string outputPath)
            {
                _writer = new StreamWriter(outputPath, false);
                _writer.AutoFlush = true;
            }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                _writer.WriteLine(format, args);
            }

            public void LogException(Exception exception, Object context)
            {
                _writer.WriteLine("{0}", new object[] { exception.ToString() });
                if (exception.StackTrace != null)
                {
                    _writer.WriteLine("{0}", new object[] { exception.StackTrace });
                }
            }

            public void Dispose()
            {
                _writer?.Dispose();
            }
        }
    }
}
