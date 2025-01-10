// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin
{
    /// <summary>
    /// Can be terminated.
    /// </summary>
    public interface ITerminatable
    {
        /// <summary>
        /// Terminate Autopilot
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
        private AutopilotState _state;
        private AutopilotSettings _settings;
        private bool _isTerminating;

        /// <summary>
        /// Returns the running Autopilot instance.
        /// No caching.
        /// </summary>
        [NotNull]
        public static Autopilot Instance
        {
            get
            {
#if UNITY_2022_3_OR_NEWER
                var autopilot = FindAnyObjectByType<Autopilot>();
                // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
                var autopilot = FindObjectOfType<Autopilot>();
#endif
                if (autopilot == null)
                {
                    throw new InvalidOperationException("Autopilot instance not found");
                }

                return autopilot;
            }
        }

        private void Start()
        {
            _state = AutopilotState.Instance;
            _settings = _state.settings;
            if (_settings == null)
            {
                throw new InvalidOperationException("Autopilot is not running");
            }

#if UNITY_EDITOR
            if (_state.launchFrom == LaunchType.Commandline)
            {
                EditorApplication.playModeStateChanged += OnExitPlayModeToTerminateEditor;
            }
            else
            {
                EditorApplication.playModeStateChanged += OnExitPlayModeToTeardown;
            }
#endif

            _logger = _settings.LoggerAsset.Logger;
            // Note: Set a default logger if no logger settings. see: AutopilotSettings.Initialize method.
            _logger.Log("Launching Autopilot...");

            if (!int.TryParse(_settings.randomSeed, out var seed))
            {
                seed = Environment.TickCount;
            }

            _randomFactory = new RandomFactory(seed);
            _logger.Log($"Random seed is {seed}");

            _dispatcher = new AgentDispatcher(_settings, _logger, _randomFactory);
            _dispatcher.DispatchSceneCrossingAgents();
            var dispatched = _dispatcher.DispatchByScene(SceneManager.GetActiveScene(), false);
            if (!dispatched)
            {
                DispatchByLoadedScenes(); // Try dispatch by loaded (not active) scenes.
            }

            if (_settings.lifespanSec > 0)
            {
                StartCoroutine(Lifespan(
                    _settings.lifespanSec,
                    _settings.ExitCode,
                    _settings.exitMessage));
            }

            if (Math.Abs(_settings.timeScale - 1.0f) > 0.001 && _settings.timeScale > 0) // 0 is ignored
            {
                Time.timeScale = _settings.timeScale;
            }

            _logger.Log("Launched Autopilot");
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
        private IEnumerator Lifespan(int timeoutSec, ExitCode exitCode, string message)
        {
            yield return new WaitForSecondsRealtime(timeoutSec);
            yield return UniTask.ToCoroutine(() => TerminateAsync(exitCode, message));
        }

        private void OnDestroy()
        {
            // Clear event listeners.
            // When play mode is stopped by the user, onDestroy calls without TerminateAsync.

            _logger?.Log("Destroy Autopilot object");
            _dispatcher?.Dispose();
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

#if UNITY_EDITOR
            if (_state.launchFrom != LaunchType.Commandline)
            {
                EditorApplication.playModeStateChanged -= OnExitPlayModeToTeardown;
            }
#endif

            if (reporting && _state.IsRunning && _settings.Reporter != null)
            {
                await _settings.Reporter.PostReportAsync(message, stackTrace, exitCode, token);
            }

            Destroy(this.gameObject);

            _logger.Log("Terminate Autopilot");
            Launcher.TeardownLaunchAutopilotAsync(_state, _logger, exitCode, message).Forget();
        }

        [Obsolete("Use " + nameof(TerminateAsync))]
        public void Terminate(ExitCode exitCode, string logString = null, string stackTrace = null)
        {
            TerminateAsync(exitCode, logString, stackTrace).Forget();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Stop Autopilot on play mode exit event when run on Unity editor.
        /// Not called when invoked from play mode (not registered in event listener).
        /// </summary>
        private static void OnExitPlayModeToTerminateEditor(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= OnExitPlayModeToTerminateEditor;

            // Exit Unity when returning from play mode to edit mode.
            // Because it may freeze when exiting without going through edit mode.
            var exitCode = (int)AutopilotState.Instance.exitCode;
            Debug.Log($"Exit Unity-editor by Autopilot, exit code: {exitCode}");
            EditorApplication.Exit(exitCode);
        }

        /// <summary>
        /// Teardown when Play Mode is stopped while the Autopilot is running.
        /// </summary>
        private static void OnExitPlayModeToTeardown(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= OnExitPlayModeToTeardown;

            // Teardown when Play Mode is stopped while the Autopilot is running.
            Debug.LogWarning("Play Mode is stopped while the Autopilot is running");
            AutopilotState.Instance.Reset();
        }
#endif
    }
}
