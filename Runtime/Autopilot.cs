﻿// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace DeNA.Anjin
{
    /// <summary>
    /// Autopilot main logic
    /// </summary>
    public class Autopilot : MonoBehaviour
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

        private ILogger _logger;
        private RandomFactory _randomFactory;
        private IAgentDispatcher _dispatcher;
        private IReporter _reporter;
        private LogMessageHandler _logMessageHandler;
        private AutopilotState _state;
        private AutopilotSettings _settings;
        private float _startTime;

        private void Start()
        {
            _state = AutopilotState.Instance;
            _settings = _state.settings;
            Assert.IsNotNull(_settings);

            _logger = new ConsoleLogger(Debug.unityLogger.logHandler);

            if (!int.TryParse(_settings.randomSeed, out var seed))
            {
                seed = Environment.TickCount;
            }

            _randomFactory = new RandomFactory(seed);
            _logger.Log($"Random seed is {seed}");

            _dispatcher = new AgentDispatcher(_settings, _logger, _randomFactory);
            _dispatcher.DispatchByScene(SceneManager.GetActiveScene());
            SceneManager.activeSceneChanged += _dispatcher.DispatchByScene;

            _reporter = new SlackReporter(_settings, new SlackAPI());
            _logMessageHandler = new LogMessageHandler(_settings, _reporter);
            Application.logMessageReceived += _logMessageHandler.HandleLog;

            if (_settings.lifespanSec > 0)
            {
                StartCoroutine(Lifespan(_settings.lifespanSec));
            }

            if (Math.Abs(_settings.timeScale - 1.0f) > 0.001 && _settings.timeScale > 0) // 0 is ignored
            {
                Time.timeScale = _settings.timeScale;
            }

            _startTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Terminate when ran specified time.
        /// </summary>
        /// <param name="timeoutSec"></param>
        /// <returns></returns>
        private IEnumerator Lifespan(int timeoutSec)
        {
            yield return new WaitForSecondsRealtime(timeoutSec);
            Terminate(ExitCode.Normally);
        }

        /// <summary>
        /// Terminate autopilot
        /// </summary>
        /// <param name="exitCode">Exit code for Unity Editor</param>
        /// <param name="logString">Log message string when terminate by the log message</param>
        /// <param name="stackTrace">Stack trace when terminate by the log message</param>
        public void Terminate(ExitCode exitCode, string logString = null, string stackTrace = null)
        {
            if (_dispatcher != null)
            {
                SceneManager.activeSceneChanged -= _dispatcher.DispatchByScene;
            }

            if (_logMessageHandler != null)
            {
                Application.logMessageReceived -= _logMessageHandler.HandleLog;
            }

            if (_state.settings != null && !string.IsNullOrEmpty(_state.settings.junitReportPath))
            {
                var time = Time.realtimeSinceStartup - _startTime;
                JUnitReporter.Output(_state.settings.junitReportPath, (int)exitCode, logString, stackTrace, time);
            }

            Destroy(this.gameObject);

            if (_state.launchFrom == AutopilotState.LaunchType.EditorPlayMode)
            {
                _logger.Log("Terminate autopilot");
                _state.Reset();
                return; // Only terminate autopilot run if starting from play mode.
            }

#if UNITY_EDITOR
            // Terminate when ran specified time.
            _logger.Log("Stop playing by autopilot");
            _state.exitCode = exitCode;
            UnityEditor.EditorApplication.isPlaying = false;
            // Call Launcher.OnChangePlayModeState() so terminates Unity editor, when launch from CLI.
#else
            _logger.Log($"Exit Unity-editor by autopilot, exit code={exitCode}");
            Application.Quit((int)exitCode);
#endif
        }
    }
}
