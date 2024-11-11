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
        /// Normally terminate.
        /// </summary>
        Normally = 0,

        /// <summary>
        /// Terminate by uncaught exceptions.
        /// </summary>
        UnCatchExceptions = 1,

        /// <summary>
        /// Terminate by <c>ErrorHandlerAgent</c>.
        /// </summary>
        DetectErrorsInLog,

        /// <summary>
        /// Terminate by Autopilot launching failure.
        /// </summary>
        AutopilotLaunchingFailed,

        /// <summary>
        /// Terminate by Autopilot scenario running failure.
        /// </summary>
        AutopilotFailed,
    }
}
