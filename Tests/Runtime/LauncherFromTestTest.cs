// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestUtils.Build;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            Assert.That(state.IsRunning, Is.False);
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayMode));
            Assert.That(state.exitCode, Is.EqualTo(ExitCode.Normally));
        }

        [Test]
        [PrebuildSetup(typeof(CopyAssetsToResources))]
        [PostBuildCleanup(typeof(CleanupResources))]
        public async Task AutopilotAsync_WithAssetFile_RunAutopilot()
        {
            var assetPath = CopyAssetsToResources.GetAssetPath("AutopilotSettingsForTests.asset"); // lifespanSec is 2

            var beforeTimestamp = Time.time;
            await LauncherFromTest.AutopilotAsync(assetPath);

            var afterTimestamp = Time.time;
            Assert.That(afterTimestamp - beforeTimestamp, Is.GreaterThan(2f), "Autopilot is running for 2 seconds");

            var state = AutopilotState.Instance;
            Assert.That(state.IsRunning, Is.False);
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayMode));
            Assert.That(state.exitCode, Is.EqualTo(ExitCode.Normally));
        }
    }
}
