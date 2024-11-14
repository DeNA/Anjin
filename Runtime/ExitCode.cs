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
        /// Normally terminated.
        /// </summary>
        Normally = 0,

        /// <summary>
        /// Terminated by uncaught exceptions.
        /// </summary>
        UnCatchExceptions = 1,

        /// <summary>
        /// Terminated by <c>ErrorHandlerAgent</c>.
        /// </summary>
        DetectErrorsInLog,

        /// <summary>
        /// Terminated by Autopilot launching failure.
        /// </summary>
        AutopilotLaunchingFailed,

        /// <summary>
        /// Terminated by Autopilot scenario running failure.
        /// </summary>
        AutopilotFailed,

        /// <summary>
        /// Terminated by Autopilot lifespan expired.
        /// By default, the exit code at expiration will be <c>Normally</c>.
        /// This exit code is only used if set by the user.
        /// </summary>
        AutopilotLifespanExpired,
    }
}
