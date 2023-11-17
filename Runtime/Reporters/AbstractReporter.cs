// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Reporter base class
    /// </summary>
    public abstract class AbstractReporter : ScriptableObject
    {
        /// <summary>
        /// Post report log message, stacktrace and screenshot
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        /// <param name="logString">Log message</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="type">Log message type</param>
        /// <param name="withScreenshot">With screenshot</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public abstract UniTask PostReportAsync(
            AutopilotSettings settings,
            string logString,
            string stackTrace,
            LogType type,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        );
    }
}
