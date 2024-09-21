// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Utilities
{
    /// <summary>
    /// Log message handling using <c>Application.logMessageReceived</c>.
    /// </summary>
    public class LogMessageHandler : IDisposable
    {
        private readonly AutopilotSettings _settings;
        private readonly AbstractReporter _reporter;
        private List<Regex> _ignoreMessagesRegexes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        /// <param name="reporter">Reporter implementation</param>
        public LogMessageHandler(AutopilotSettings settings, AbstractReporter reporter)
        {
            _settings = settings;
            _reporter = reporter;
            Application.logMessageReceivedThreaded += this.HandleLog;
        }

        public void Dispose()
        {
            Application.logMessageReceivedThreaded -= this.HandleLog;
        }

        /// <summary>
        /// Handle log message and post notification to Reporter (if necessary).
        /// </summary>
        /// <param name="logString">Log message string</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="type">Log message type</param>
        public async void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (IsIgnoreMessage(logString, stackTrace, type, _settings))
            {
                return;
            }

            // NOTE: HandleLog may called by non-main thread because it subscribe Application.logMessageReceivedThreaded
            await UniTask.SwitchToMainThread();

            if (_reporter != null)
            {
                await _reporter.PostReportAsync(logString, stackTrace, type, true);
            }

            var autopilot = Object.FindObjectOfType<Autopilot>();
            if (autopilot != null)
            {
                await autopilot.TerminateAsync(ExitCode.AutopilotFailed, logString, stackTrace);
            }
        }

        private bool IsIgnoreMessage(string logString, string stackTrace, LogType type,
            AutopilotSettings settings)
        {
            if (type == LogType.Log)
            {
                return true;
            }

            if (type == LogType.Exception && !settings.handleException)
            {
                return true;
            }

            if (type == LogType.Assert && !settings.handleAssert)
            {
                return true;
            }

            if (type == LogType.Error && !settings.handleError)
            {
                return true;
            }

            if (type == LogType.Warning && !settings.handleWarning)
            {
                return true;
            }

            if (_ignoreMessagesRegexes == null)
            {
                _ignoreMessagesRegexes = CreateIgnoreMessageRegexes(_settings);
            }

            if (_ignoreMessagesRegexes.Exists(regex => regex.IsMatch(logString)))
            {
                return true;
            }

            if (stackTrace.Contains(nameof(LogMessageHandler)) || stackTrace.Contains(nameof(SlackAPI)))
            {
                Debug.Log($"Ignore looped message: {logString}");
                return true;
            }

            return false;
        }

        private static List<Regex> CreateIgnoreMessageRegexes(AutopilotSettings settings)
        {
            var regexs = new List<Regex>();
            foreach (var message in settings.ignoreMessages)
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
    }
}
