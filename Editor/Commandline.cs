// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Reflection;
using DeNA.Anjin.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Scripting;

namespace DeNA.Anjin.Editor
{
    /// <summary>
    /// Launch from Commandline interface
    /// </summary>
    public static class Commandline
    {
        /// <summary>
        /// Launch Unity editor from the CLI, change play mode and run autopilot.
        /// </summary>
        [Preserve]
        private static void Bootstrap()
        {
            Debug.Log("Enter Commandline.Bootstrap() !!!");

            var settingsFile = GetAutopilotSettingsFromArguments();
            var settings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(settingsFile);
            if (!settings)
            {
                Debug.LogError($"Autopilot not be started. Because no AutopilotSettings instance. path={settingsFile}");
                EditorApplication.Exit((int)ExitCode.AutopilotFailed);
                return;
            }

            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError(
                    $"Autopilot not be started. Because undefined startup scene. Please set \"Build Settings... | Scenes In Build\".");
                EditorApplication.Exit((int)ExitCode.AutopilotFailed);
                return;
            }

            var startScenePath = EditorBuildSettings.scenes[0].path;
            var myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startScenePath);
            if (myWantedStartScene == null)
            {
                Debug.LogError($"Autopilot not be started. Because could not find scene {startScenePath}.");
                EditorApplication.Exit((int)ExitCode.AutopilotFailed);
                return;
            }

            // Show GameView window even in batchmode. It can bypass batchmode limitations. e.g., WaitForEndOfFrame.
            FocusGameView();

            // Set first open Scene
            EditorSceneManager.playModeStartScene = myWantedStartScene;

            // Apply commandline arguments
            settings.OverrideByCommandLineArguments(new Arguments());

            // Register event handler for terminate autopilot
            EditorApplication.playModeStateChanged += OnChangePlayModeState;

            // Activate autopilot and enter play mode
            var state = AutopilotState.Instance;
            state.launchFrom = LaunchType.Commandline;
            state.settings = settings;
            EditorApplication.isPlaying = true;

            // After this method finishes, executed the following order in play mode
            //  1. Load the specified Scene
            //  2. Execute `Awake()` method of the GameObject in the Scene
            //  3. Execute methods with `RuntimeInitializeOnLoadMethodAttribute`, in Anjin `Autopilot.Run()`
            //  4. Execute `Start()` method of the GameObject in the Scene

            // Note: that if `-quit` is specified in the CLI, it will end without being executed.

            Debug.Log("Exit Commandline.Bootstrap() !!!");
        }

        private static string GetAutopilotSettingsFromArguments()
        {
            var args = new Arguments();

            if (!args.AutopilotSettings.IsCaptured())
            {
                Debug.LogError("Autopilot not be started. Because not specified AutopilotSettings.");
                EditorApplication.Exit(1);
            }

            return args.AutopilotSettings.Value();
        }

        private static void FocusGameView()
        {
            var assembly = Assembly.Load("UnityEditor.dll");
            var viewClass = Application.isBatchMode
                ? "UnityEditor.GameView"
                : "UnityEditor.PlayModeView";
            var gameView = assembly.GetType(viewClass);
            EditorWindow.GetWindow(gameView, false, null, true);
        }

        /// <summary>
        /// Stop autopilot on play mode exit event when run on Unity editor.
        /// Not called when invoked from play mode (not registered in event listener).
        /// </summary>
        /// <param name="playModeStateChange"></param>
        private static void OnChangePlayModeState(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= OnChangePlayModeState;

            // Exit Unity when returning from play mode to edit mode.
            // Because it may freeze when exiting without going through edit mode.
            var exitCode = (int)AutopilotState.Instance.exitCode;
            Debug.Log($"Exit Unity-editor by autopilot, exit code: {(int)exitCode}");
            EditorApplication.Exit(exitCode);
        }
    }
}
