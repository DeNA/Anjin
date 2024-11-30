// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Autopilot exit code that terminates by <c>TerminateAgent</c>.
    /// </summary>
    public enum ExitCodeByTerminateAgent
    {
        /// <summary>
        /// Normally terminated.
        /// </summary>
        Normally = ExitCode.Normally,

        /// <summary>
        /// Terminate by Autopilot scenario running failure.
        /// By default, the exit code at expiration will be <c>Normally</c>.
        /// This exit code is only used if set by the user.
        /// </summary>
        AutopilotFailed = ExitCode.AutopilotFailed,

        /// <summary>
        /// Input custom exit code by integer value.
        /// </summary>
        Custom = 1024,
    }

    /// <summary>
    /// An Agent that terminates the Autopilot running when the scenario goal is achieved.
    /// </summary>
    [CreateAssetMenu(fileName = "New TerminateAgent", menuName = "Anjin/Terminate Agent", order = 22)]
    public class TerminateAgent : AbstractAgent
    {
        /// <summary>
        /// Exit Code when terminated by this Agent.
        /// When using it from within code, use the <code>ExitCode</code> property.
        /// </summary>
        public ExitCodeByTerminateAgent exitCode = ExitCodeByTerminateAgent.Normally;

        /// <summary>
        /// Custom exit code to be used if <code>Custom</code> is selected for <c>exitCode</c>.
        /// Please enter an integer value.
        /// </summary>
        public string customExitCode;

        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        public ExitCode ExitCode
        {
            get
            {
                if (exitCode == ExitCodeByTerminateAgent.Custom)
                {
                    return int.TryParse(customExitCode, out var intExitCode)
                        ? (ExitCode)intExitCode
                        : (ExitCode)exitCode;
                }

                return (ExitCode)exitCode;
            }
        }

        /// <summary>
        /// Message used by Reporter.
        /// </summary>
        [Multiline]
        public string exitMessage = "Terminated by TerminateAgent";

        /// <inheritdoc/>
        public override async UniTask Run(CancellationToken token)
        {
            try
            {
                Logger.Log($"Enter {this.name}.Run()");

                // ReSharper disable once MethodSupportsCancellation; Do not use this Agent's CancellationToken.
                AutopilotInstance.TerminateAsync(ExitCode, exitMessage).Forget();
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }

            await UniTask.CompletedTask;
        }
    }
}
