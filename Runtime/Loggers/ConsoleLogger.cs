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
        public ConsoleLogger()
        {
            LoggerImpl = new Logger(Debug.unityLogger.logHandler);
            LoggerImpl.filterLogType = LogType.Log;
        }
    }
}
