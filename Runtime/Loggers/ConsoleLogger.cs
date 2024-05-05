// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// Logger using <c>Debug.unityLogger</c>
    /// </summary>
    [CreateAssetMenu(fileName = "New ConsoleLogger", menuName = "Anjin/Console Logger", order = 71)]
    public class ConsoleLogger : AbstractLogger
    {
        /// <summary>
        /// To selective enable debug log message.
        /// </summary>
        public LogType filterLogType = LogType.Log;

        /// <inheritdoc />
        public override ILogger LoggerImpl =>
            new Logger(Debug.unityLogger.logHandler) { filterLogType = filterLogType };
    }
}
