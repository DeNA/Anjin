// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Loggers;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using AssertionException = NUnit.Framework.AssertionException;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin
{
    /// <summary>
    /// Autopilot main logic
    /// </summary>
    public class Autopilot : MonoBehaviour
    {
        private AbstractLoggerAsset _loggerAsset;
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

            _loggerAsset = _settings.loggerAsset;
            _logger = _loggerAsset != null ? _loggerAsset.Logger : CreateDefaultLogger();

            ConvertSlackReporterFromObsoleteSlackSettings(_settings, _logger);

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
        /// Returns an logger that autopilot uses. You can change a logger by overriding this method.
        /// Default logger is that write to console.
        /// </summary>
        /// <returns>A new logger that write to console</returns>
        private static ILogger CreateDefaultLogger()
        {
            return Debug.unityLogger;
        }

        [Obsolete("Remove this method when bump major version")]
        internal static void ConvertSlackReporterFromObsoleteSlackSettings(AutopilotSettings settings, ILogger logger)
        {
            if (string.IsNullOrEmpty(settings.slackToken) || string.IsNullOrEmpty(settings.slackChannels) ||
                settings.reporter != null)
                // TODO: This condition will change when the AutopilotSettings.reporter is changed to a List<AbstractReporter>.
            {
                return;
            }

            logger.Log(LogType.Warning, @"Slack settings in AutopilotSettings has been obsoleted.
Please delete the value using Debug Mode in the Inspector window. And create a SlackReporter asset file.
This time, temporarily generate and use SlackReporter instance.");

            var reporter = ScriptableObject.CreateInstance<SlackReporter>();
            reporter.slackToken = settings.slackToken;
            reporter.slackChannels = settings.slackChannels;
            reporter.mentionSubTeamIDs = settings.mentionSubTeamIDs;
            reporter.addHereInSlackMessage = settings.addHereInSlackMessage;
            settings.reporter = reporter;
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
            _settings.loggerAsset?.Dispose();
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

            Destroy(this.gameObject);

            _logger.Log("Terminate autopilot");
            _state.settings = null;
            _state.exitCode = exitCode;

            if (_state.launchFrom == LaunchType.PlayMode) // Note: Editor play mode, Play mode tests, and Player build
            {
#if UNITY_INCLUDE_TESTS
                // Play mode tests
                if (TestContext.CurrentContext != null && exitCode != ExitCode.Normally)
                {
                    throw new AssertionException($"Autopilot failed with exit code {exitCode}");
                }
#endif
                return; // Only terminate autopilot run if starting from play mode.
            }

#if UNITY_EDITOR
            // Terminate when launch from edit mode (including launch from commandline)
            _logger.Log("Stop playing by autopilot");
            // XXX: Avoid a problem that Editor stay playing despite isPlaying get assigned false.
            // SEE: https://github.com/DeNA/Anjin/issues/20
            await UniTask.NextFrame(token);
            EditorApplication.isPlaying = false;
            // Note: Call `Launcher.OnChangePlayModeState()` so terminates Unity editor, when launch from commandline.
#else
            // Player build launch from commandline
            _logger.Log($"Exit Unity-player by autopilot, exit code={exitCode}");
            Application.Quit((int)exitCode);
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
