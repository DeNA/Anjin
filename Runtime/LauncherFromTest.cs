// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;

namespace DeNA.Anjin
{
    /// <summary>
    /// Launch from Play Mode test interface.
    /// </summary>
    [Obsolete("Use `Launcher.LaunchAutopilotAsync` instead")]
    public static class LauncherFromTest
    {
        /// <summary>
        /// Run autopilot from Play Mode test.
        /// If an error is detected in running, it will be output to `LogError` and the test will fail.
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        [Obsolete("Use `Launcher.LaunchAutopilotAsync` instead")]
        public static async UniTask AutopilotAsync(AutopilotSettings settings)
        {
            await Launcher.LaunchAutopilotAsync(settings);
        }

        /// <summary>
        /// Run autopilot from Play Mode test.
        /// If an error is detected in running, it will be output to `LogError` and the test will fail.
        /// </summary>
        /// <param name="autopilotSettingsPath">Asset file path for autopilot settings</param>
        [Obsolete("Use `Launcher.LaunchAutopilotAsync` instead")]
        public static async UniTask AutopilotAsync(string autopilotSettingsPath)
        {
            await Launcher.LaunchAutopilotAsync(autopilotSettingsPath);
        }
    }
}
