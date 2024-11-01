// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Utilities
{
    /// <summary>
    /// Log message handling using <c>Application.logMessageReceived</c>.
    /// </summary>
    public class LogMessageHandler : IDisposable
    {
        private readonly AutopilotSettings _settings;
        private readonly ITerminatable _autopilot;

        private List<Regex> _ignoreMessagesRegexes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        /// <param name="autopilot">Autopilot instance that termination target</param>
        public LogMessageHandler(AutopilotSettings settings, ITerminatable autopilot)
        {
            _settings = settings;
            _autopilot = autopilot;

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
            if (_settings == null)
            {
                return;
            }

            if (IsIgnoreMessage(logString, stackTrace, type))
            {
                return;
            }

            // NOTE: HandleLog may called by non-main thread because it subscribe Application.logMessageReceivedThreaded
            await UniTask.SwitchToMainThread();

            if (_settings.loggerAsset != null)
            {
                _settings.loggerAsset.Logger.Log(type, logString, stackTrace);
            }

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

            if (type == LogType.Exception && !_settings.handleException)
            {
                return true;
            }

            if (type == LogType.Assert && !_settings.handleAssert)
            {
                return true;
            }

            if (type == LogType.Error && !_settings.handleError)
            {
                return true;
            }

            if (type == LogType.Warning && !_settings.handleWarning)
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
