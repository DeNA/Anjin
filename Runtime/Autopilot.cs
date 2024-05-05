// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private ILogger _logger;
        private RandomFactory _randomFactory;
        private IAgentDispatcher _dispatcher;
        private LogMessageHandler _logMessageHandler;
        private AutopilotState _state;
        private AutopilotSettings _settings;
        private float _startTime;

        private void Start()
        {
            _state = AutopilotState.Instance;
            _settings = _state.settings;
            Assert.IsNotNull(_settings);

            if (_settings.logger != null)
            {
                _logger = _settings.logger.LoggerImpl;
            }
            else
            {
                _logger = CreateDefaultLogger();
            }

            if (!int.TryParse(_settings.randomSeed, out var seed))
            {
                seed = Environment.TickCount;
            }

            _randomFactory = new RandomFactory(seed);
            _logger.Log($"Random seed is {seed}");

            // NOTE: Registering logMessageReceived must be placed before DispatchByScene.
            //       Because some agent can throw an error immediately, so reporter can miss the error if
            //       registering logMessageReceived is placed after DispatchByScene.
            _logMessageHandler = new LogMessageHandler(_settings, _settings.reporter);

            _dispatcher = new AgentDispatcher(_settings, _logger, _randomFactory);
            _dispatcher.DispatchByScene(SceneManager.GetActiveScene());

            if (_settings.lifespanSec > 0)
            {
                StartCoroutine(Lifespan(_settings.lifespanSec));
            }

            if (Math.Abs(_settings.timeScale - 1.0f) > 0.001 && _settings.timeScale > 0) // 0 is ignored
            {
                Time.timeScale = _settings.timeScale;
            }

            _startTime = Time.realtimeSinceStartup;
            _logger.Log("Launched autopilot");
        }

        /// <summary>
        /// Returns an logger that autopilot uses. You can change a logger by overriding this method.
        /// Default logger is that write to console.
        /// </summary>
        /// <returns>A new logger that write to console</returns>
        protected virtual ILogger CreateDefaultLogger()
        {
            return Debug.unityLogger;
        }

        /// <summary>
        /// Terminate when ran specified time.
        /// </summary>
        /// <param name="timeoutSec"></param>
        /// <returns></returns>
        private IEnumerator Lifespan(int timeoutSec)
        {
            yield return new WaitForSecondsRealtime(timeoutSec);
            yield return UniTask.ToCoroutine(() => TerminateAsync(ExitCode.Normally));
        }

        private void OnDestroy()
        {
            // Clear event listeners.
            // When play mode is stopped by the user, onDestroy calls without TerminateAsync.

            _dispatcher?.Dispose();
            _logMessageHandler?.Dispose();
            _settings.logger?.Dispose();
        }

        /// <summary>
        /// Terminate autopilot
        /// </summary>
        /// <param name="exitCode">Exit code for Unity Editor</param>
        /// <param name="logString">Log message string when terminate by the log message</param>
        /// <param name="stackTrace">Stack trace when terminate by the log message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A task awaits termination get completed</returns>
        public async UniTask TerminateAsync(ExitCode exitCode, string logString = null, string stackTrace = null,
            CancellationToken token = default)
        {
            if (_state.settings != null && !string.IsNullOrEmpty(_state.settings.junitReportPath))
            {
                var time = Time.realtimeSinceStartup - _startTime;
                JUnitReporter.Output(_state.settings.junitReportPath, (int)exitCode, logString, stackTrace, time);
            }

            if (exitCode != ExitCode.Normally)
            {
                _logger.Log(LogType.Error, logString + Environment.NewLine + stackTrace);
            }

            Destroy(this.gameObject);

            if (_state.IsLaunchFromPlayMode)
            {
                _logger.Log("Terminate autopilot");
                _state.Reset();
#if UNITY_INCLUDE_TESTS
                if (_state.launchFrom == LaunchType.PlayModeTests && exitCode != ExitCode.Normally)
                {
                    throw new NUnit.Framework.AssertionException($"Autopilot failed with exit code {exitCode}");
                }
#endif
                return; // Only terminate autopilot run if starting from play mode.
            }

#if UNITY_EDITOR
            // Terminate when ran specified time.
            _logger.Log("Stop playing by autopilot");
            _state.exitCode = exitCode;
            // XXX: Avoid a problem that Editor stay playing despite isPlaying get assigned false.
            // SEE: https://github.com/DeNA/Anjin/issues/20
            await UniTask.DelayFrame(1, cancellationToken: token);
            UnityEditor.EditorApplication.isPlaying = false;
            // Call Launcher.OnChangePlayModeState() so terminates Unity editor, when launch from CLI.
#else
            _logger.Log($"Exit Unity-editor by autopilot, exit code={exitCode}");
            Application.Quit((int)exitCode);
            await UniTask.CompletedTask;
#endif
        }

        /// <summary>
        /// Terminate autopilot
        /// </summary>
        /// <param name="exitCode">Exit code for Unity Editor</param>
        /// <param name="logString">Log message string when terminate by the log message</param>
        /// <param name="stackTrace">Stack trace when terminate by the log message</param>
        [Obsolete("Use " + nameof(TerminateAsync))]
        public void Terminate(ExitCode exitCode, string logString = null, string stackTrace = null)
        {
            TerminateAsync(exitCode, logString, stackTrace).Forget();
        }
    }
}
