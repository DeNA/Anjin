// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

namespace DeNA.Anjin
{
    /// <summary>
    /// Can be terminated.
    /// </summary>
    public interface ITerminatable
    {
        /// <summary>
        /// Terminate autopilot
        /// </summary>
        /// <param name="exitCode">Exit code for Unity Editor/ Player-build</param>
        /// <param name="message">Log message string or terminate message</param>
        /// <param name="stackTrace">Stack trace when terminate by the log message</param>
        /// <param name="reporting">Call Reporter if true</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A task awaits termination get completed</returns>
        UniTask TerminateAsync(ExitCode exitCode, string message = null, string stackTrace = null,
            bool reporting = true, CancellationToken token = default);
    }

    /// <summary>
    /// Autopilot main logic
    /// </summary>
    public class Autopilot : MonoBehaviour, ITerminatable
    {
        private ILogger _logger;
        private RandomFactory _randomFactory;
        private IAgentDispatcher _dispatcher;
        private LogMessageHandler _logMessageHandler;
        private AutopilotState _state;
        private AutopilotSettings _settings;
        private float _startTime;
        private bool _isTerminating;

        private void Start()
        {
            _state = AutopilotState.Instance;
            _settings = _state.settings;
            Assert.IsNotNull(_settings);

            _logger = _settings.LoggerAsset.Logger;
            // Note: Set a default logger if no logger settings. see: AutopilotSettings.Initialize method.

            if (!int.TryParse(_settings.randomSeed, out var seed))
            {
                seed = Environment.TickCount;
            }

            _randomFactory = new RandomFactory(seed);
            _logger.Log($"Random seed is {seed}");

            // NOTE: Registering logMessageReceived must be placed before DispatchByScene.
            //       Because some agent can throw an error immediately, so reporter can miss the error if
            //       registering logMessageReceived is placed after DispatchByScene.
            _logMessageHandler = new LogMessageHandler(_settings, this);

            _dispatcher = new AgentDispatcher(_settings, _logger, _randomFactory);
            _dispatcher.DispatchSceneCrossingAgents();
            var dispatched = _dispatcher.DispatchByScene(SceneManager.GetActiveScene(), false);
            if (!dispatched)
            {
                DispatchByLoadedScenes(); // Try dispatch by loaded (not active) scenes.
            }

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

        private void DispatchByLoadedScenes()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var fallback = i == SceneManager.sceneCount - 1; // Use fallback agent only in the last scene.
                if (_dispatcher.DispatchByScene(SceneManager.GetSceneAt(i), fallback))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Terminate when ran specified time.
        /// </summary>
        /// <param name="timeoutSec"></param>
        /// <returns></returns>
        private IEnumerator Lifespan(int timeoutSec)
        {
            yield return new WaitForSecondsRealtime(timeoutSec);
            yield return UniTask.ToCoroutine(() =>
                TerminateAsync(ExitCode.Normally, "Autopilot has reached the end of its lifespan."));
        }

        private void OnDestroy()
        {
            // Clear event listeners.
            // When play mode is stopped by the user, onDestroy calls without TerminateAsync.

            _logger?.Log("Destroy Autopilot object");
            _dispatcher?.Dispose();
            _logMessageHandler?.Dispose();
            _settings.LoggerAsset?.Dispose();
        }

        /// <inheritdoc/>
        public async UniTask TerminateAsync(ExitCode exitCode, string message = null, string stackTrace = null,
            bool reporting = true, CancellationToken token = default)
        {
            if (_isTerminating)
            {
                return; // Prevent multiple termination.
            }

            _isTerminating = true;

            if (reporting && _state.IsRunning && _settings.Reporter != null)
            {
                await _settings.Reporter.PostReportAsync(message, stackTrace, exitCode, token);
            }

            if (_state.settings != null && !string.IsNullOrEmpty(_state.settings.junitReportPath))
            {
                var time = Time.realtimeSinceStartup - _startTime;
                JUnitReporter.Output(_state.settings.junitReportPath, (int)exitCode, message, stackTrace, time);
            }

            Destroy(this.gameObject);

            _logger.Log("Terminate Autopilot");
            Launcher.TeardownLaunchAutopilotAsync(_state, _logger, exitCode, "Autopilot").Forget();
        }

        [Obsolete("Use " + nameof(TerminateAsync))]
        public void Terminate(ExitCode exitCode, string logString = null, string stackTrace = null)
        {
            TerminateAsync(exitCode, logString, stackTrace).Forget();
        }
    }
}
