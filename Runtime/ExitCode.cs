// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin
{
    /// <summary>
    /// Exit code for autopilot running
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// Normally exit
        /// </summary>
        Normally = 0,

        /// <summary>
        /// Exit by un catch Exceptions
        /// </summary>
        UnCatchExceptions = 1,

        /// <summary>
        /// Exit by fault in log message
        /// </summary>
        AutopilotFailed = 2
    }
}
