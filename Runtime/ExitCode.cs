// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin
{
    /// <summary>
    /// Autopilot exit code.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// Normally exit.
        /// </summary>
        Normally = 0,

        /// <summary>
        /// Uncaught exceptions.
        /// </summary>
        UnCatchExceptions = 1,

        /// <summary>
        /// Autopilot running failed.
        /// </summary>
        AutopilotFailed = 2,

        /// <summary>
        /// Autopilot launching failed.
        /// </summary>
        AutopilotLaunchingFailed
    }
}
