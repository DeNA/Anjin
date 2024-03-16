// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin
{
    [TestFixture]
    public class LauncherFromTestTest
    {
        [Test]
        public async Task AutopilotAsync_RunAutopilot()
        {
            var agent = ScriptableObject.CreateInstance(typeof(DoNothingAgent)) as DoNothingAgent;
            agent.lifespanSec = 1;

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.fallbackAgent = agent;
            settings.lifespanSec = 2;

            var beforeTimestamp = Time.time;
            await LauncherFromTest.AutopilotAsync(settings);

            var afterTimestamp = Time.time;
            Assert.That(afterTimestamp - beforeTimestamp, Is.GreaterThan(2f), "Autopilot is running for 2 seconds");

            var state = AutopilotState.Instance;
            Assert.That(state.IsRunning, Is.False, "AutopilotState is terminated");
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayModeTests), "Launch from");
            Assert.That(state.exitCode, Is.EqualTo(ExitCode.Normally), "Exit code");
            Assert.That(EditorApplication.isPlaying, Is.True, "Keep play mode");
        }

        [Test]
        public async Task AutopilotAsync_WithAssetFile_RunAutopilot()
        {
            const string AssetPath = "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset";

            var beforeTimestamp = Time.time;
            await LauncherFromTest.AutopilotAsync(AssetPath);

            var afterTimestamp = Time.time;
            Assert.That(afterTimestamp - beforeTimestamp, Is.GreaterThan(2f), "Autopilot is running for 2 seconds");

            var state = AutopilotState.Instance;
            Assert.That(state.IsRunning, Is.False, "AutopilotState is terminated");
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayModeTests), "Launch from");
            Assert.That(state.exitCode, Is.EqualTo(ExitCode.Normally), "Exit code");
            Assert.That(EditorApplication.isPlaying, Is.True, "Keep play mode");
        }
    }
}
