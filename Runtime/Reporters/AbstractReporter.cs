// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Reporter base class.
    /// Reporter is called on Autopilot termination.
    /// </summary>
    public abstract class AbstractReporter : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Description about this agent instance.
        /// </summary>
        [Multiline] public string description;
#endif

        /// <summary>
        /// Post report log message, stacktrace and screenshot
        /// </summary>
        /// <param name="message">Log message or terminate message</param>
        /// <param name="stackTrace">Stack trace (can be null)</param>
        /// <param name="exitCode">Exit code indicating the reason for termination</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public abstract UniTask PostReportAsync(
            string message,
            string stackTrace,
            ExitCode exitCode,
            CancellationToken cancellationToken = default
        );
    }
}
