// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Reporter interface
    /// </summary>
    public interface IReporter
    {
        /// <summary>
        /// Post report log message, stacktrace and screenshot
        /// </summary>
        /// <param name="logString">Log message</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="type">Log message type</param>
        /// <param name="withScreenshot">With screenshot</param>
        /// <returns></returns>
        UniTask PostReportAsync(string logString, string stackTrace, LogType type, bool withScreenshot);
    }
}
