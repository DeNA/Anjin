// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// A Logger that outputs to a specified file.
    /// </summary>
    /// <remarks>
    /// If you want more functionality in the future, consider using the Unity Logging package (com.unity.logging) or ZLogger.
    /// <see href="https://docs.unity3d.com/Packages/com.unity.logging@latest"/>
    /// <see href="https://github.com/Cysharp/ZLogger"/>
    /// </remarks>
    [CreateAssetMenu(fileName = "New FileLogger", menuName = "Anjin/File Logger", order = 72)]
    public class FileLoggerAsset : AbstractLoggerAsset
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

        /// <summary>
        /// Output timestamp to log entities.
        /// </summary>
        public bool timestamp = true;

        private FileLogHandler _handler;
        private ILogger _logger;

        /// <inheritdoc />
        public override ILogger Logger
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                var args = new Arguments();
                string path;
                if (args.FileLoggerOutputPath.IsCaptured())
                {
                    path = args.FileLoggerOutputPath.Value();
                }
                else if (!string.IsNullOrEmpty(outputPath))
                {
                    path = PathUtils.GetAbsolutePath(outputPath);
                }
                else
                {
                    throw new InvalidOperationException("outputPath is not set.");
                }

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _handler = new FileLogHandler(path, timestamp);
                _logger = new Logger(_handler) { filterLogType = filterLogType };
                return _logger;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _handler?.Dispose();
        }

        private class TimestampCache
        {
            private string _timestampCache;
            private int _timestampCacheFrame;

            public string GetTimestamp()
            {
                if (_timestampCacheFrame != Time.frameCount)
                {
                    _timestampCache = DateTime.Now.ToString("[HH:mm:ss.fff] ");
                    _timestampCacheFrame = Time.frameCount;
                }

                return _timestampCache;
            }
        }

        private class FileLogHandler : ILogHandler, IDisposable
        {
            private readonly StreamWriter _writer;
            private readonly bool _timestamp;
            private readonly TimestampCache _timestampCache;
            private bool _disposed;

            public FileLogHandler(string outputPath, bool timestamp)
            {
                _writer = new StreamWriter(outputPath, false);
                _writer.AutoFlush = true;
                _timestamp = timestamp;
                if (_timestamp)
                {
                    _timestampCache = new TimestampCache();
                }
            }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                if (_disposed)
                {
                    return;
                }

                if (_timestamp)
                {
                    _writer.Write(_timestampCache.GetTimestamp());
                }

                _writer.WriteLine(format, args);
            }

            public void LogException(Exception exception, Object context)
            {
                if (_disposed)
                {
                    return;
                }

                if (_timestamp)
                {
                    _writer.Write(_timestampCache.GetTimestamp());
                }

                _writer.WriteLine("{0}", new object[] { exception.ToString() });
            }

            public void Dispose()
            {
                _writer?.Dispose();
                _disposed = true;
            }
        }

        [InitializeOnLaunchAutopilot(InitializeOnLaunchAutopilotAttribute.InitializeLoggerOrder)]
        public static void ResetLoggers()
        {
            // Reset runtime instances
            var loggerAssets = FindObjectsOfType<FileLoggerAsset>();
            foreach (var current in loggerAssets)
            {
                current._handler?.Dispose();
                current._handler = null;
                current._logger = null;
            }
        }
    }
}
