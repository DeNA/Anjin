// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters.Slack;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Error log handling using <c>Application.logMessageReceived</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "New ErrorHandlerAgent", menuName = "Anjin/Error Handler Agent", order = 50)]
    public class ErrorHandlerAgent : AbstractAgent
    {
        /// <summary>
        /// Slack notification when Exception detected in log
        /// </summary>
        public bool handleException = true;

        /// <summary>
        /// Slack notification when Error detected in log
        /// </summary>
        public bool handleError = true;

        /// <summary>
        /// Slack notification when Assert detected in log
        /// </summary>
        public bool handleAssert = true;

        /// <summary>
        /// Slack notification when Warning detected in log
        /// </summary>
        public bool handleWarning;

        /// <summary>
        /// Do not send Slack notifications when log messages contain this string
        /// </summary>
        public string[] ignoreMessages = new string[] { };

        internal ITerminatable _autopilot;
        private List<Regex> _ignoreMessagesRegexes;

        public override async UniTask Run(CancellationToken token)
        {
            try
            {
                Logger.Log($"Enter {this.name}.Run()");

                OverwriteByCommandLineArguments(new Arguments());

                if (_autopilot == null)
                {
                    _autopilot = FindObjectOfType<Autopilot>();
                }

                Application.logMessageReceivedThreaded += this.HandleLog;

                await UniTask.WaitWhile(() => true, cancellationToken: token);
            }
            finally
            {
                Application.logMessageReceivedThreaded -= this.HandleLog;
                Logger.Log($"Exit {this.name}.Run()");
            }
        }

        /// <summary>
        /// Handle log message and post notification to Reporter (if necessary).
        /// </summary>
        /// <param name="logString">Log message string</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="type">Log message type</param>
        internal async void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (IsIgnoreMessage(logString, stackTrace, type))
            {
                return;
            }

            // NOTE: HandleLog may called by non-main thread because it subscribe Application.logMessageReceivedThreaded
            await UniTask.SwitchToMainThread();

            Logger.Log(type, logString, stackTrace);

            if (type == LogType.Exception)
            {
                await _autopilot.TerminateAsync(ExitCode.UnCatchExceptions, logString, stackTrace);
            }
            else
            {
                await _autopilot.TerminateAsync(ExitCode.AutopilotFailed, logString, stackTrace);
            }
        }

        private bool IsIgnoreMessage(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Log)
            {
                return true;
            }

            if (type == LogType.Exception && !this.handleException)
            {
                return true;
            }

            if (type == LogType.Assert && !this.handleAssert)
            {
                return true;
            }

            if (type == LogType.Error && !this.handleError)
            {
                return true;
            }

            if (type == LogType.Warning && !this.handleWarning)
            {
                return true;
            }

            if (this._ignoreMessagesRegexes == null)
            {
                this._ignoreMessagesRegexes = CreateIgnoreMessageRegexes();
            }

            if (this._ignoreMessagesRegexes.Exists(regex => regex.IsMatch(logString)))
            {
                return true;
            }

            if (stackTrace.Contains(nameof(ErrorHandlerAgent)) || stackTrace.Contains(nameof(SlackAPI)))
            {
                Debug.Log($"Ignore looped message: {logString}");
                return true;
            }

            return false;
        }

        private List<Regex> CreateIgnoreMessageRegexes()
        {
            var regexs = new List<Regex>();
            foreach (var message in this.ignoreMessages)
            {
                Regex pattern;
                try
                {
                    pattern = new Regex(message);
                }
                catch (ArgumentException e)
                {
                    Debug.LogException(e);
                    continue;
                }

                regexs.Add(pattern);
            }

            return regexs;
        }

        internal void OverwriteByCommandLineArguments(Arguments args)
        {
            if (args.HandleException.IsCaptured())
            {
                this.handleException = args.HandleException.Value();
            }

            if (args.HandleError.IsCaptured())
            {
                this.handleError = args.HandleError.Value();
            }

            if (args.HandleAssert.IsCaptured())
            {
                this.handleAssert = args.HandleAssert.Value();
            }

            if (args.HandleWarning.IsCaptured())
            {
                this.handleWarning = args.HandleWarning.Value();
            }
        }
    }
}
