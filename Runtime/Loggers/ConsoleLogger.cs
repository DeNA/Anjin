// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// A Logger that outputs to a console.
    /// Logger using <c>Debug.unityLogger</c>
    /// </summary>
    [CreateAssetMenu(fileName = "New ConsoleLogger", menuName = "Anjin/Console Logger", order = 71)]
    public class ConsoleLogger : AbstractLogger
    {
        /// <summary>
        /// To selective enable debug log message.
        /// </summary>
        public LogType filterLogType = LogType.Log;

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

                _logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = filterLogType };
                return _logger;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
