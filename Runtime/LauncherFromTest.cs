// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin
{
    /// <summary>
    /// Launch from Play Mode test interface.
    /// </summary>
    public static class LauncherFromTest
    {
        /// <summary>
        /// Run autopilot from Play Mode test.
        /// If an error is detected in running, it will be output to `LogError` and the test will fail.
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        public static async UniTask AutopilotAsync(AutopilotSettings settings)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                throw new AssertionException("Not support run on Edit Mode tests");
            }
#endif
            var state = AutopilotState.Instance;
            if (state.IsRunning)
            {
                throw new AssertionException("Autopilot is already running");
            }

            state.launchFrom = LaunchType.PlayModeTests;
            state.settings = settings;
            Launcher.Run();

            await UniTask.WaitUntil(() => !state.IsRunning);
        }

        /// <summary>
        /// Run autopilot from Play Mode test.
        /// If an error is detected in running, it will be output to `LogError` and the test will fail.
        /// </summary>
        /// <param name="autopilotSettingsPath">Asset file path for autopilot settings</param>
        public static async UniTask AutopilotAsync(string autopilotSettingsPath)
        {
            var settings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(autopilotSettingsPath);
            await AutopilotAsync(settings);
        }
    }
}
