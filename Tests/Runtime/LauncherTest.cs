// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.TestUtils.Build;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin
{
    /// <summary>
    /// Launch autopilot tests from Play Mode.
    /// </summary>
    /// <remarks>
    /// Note: Test cases of launch/terminate from Edit Mode are in <see cref="Editor.UI.Settings.AutopilotSettingsEditorTest"/>.
    /// </remarks>
#if !UNITY_2020_1_OR_NEWER
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor)] // Fail on Unity 2019 Linux editor
#endif
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class LauncherTest
    {
        [SetUp]
        public void SetUp()
        {
            Assume.That(AutopilotState.Instance.IsRunning, Is.False);
        }

        private static AutopilotSettings CreateAutopilotSettings(int lifespanSec)
        {
            var settings = (AutopilotSettings)ScriptableObject.CreateInstance(typeof(AutopilotSettings));
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = lifespanSec;
            return settings;
        }

        [Test]
        public async Task LaunchAutopilotAsync_RunAutopilot()
        {
            var beforeTimestamp = Time.time;
            var settings = CreateAutopilotSettings(2);
            await Launcher.LaunchAutopilotAsync(settings);

            var runningTime = Time.time - beforeTimestamp;
            Assert.That(runningTime, Is.GreaterThan(2f).And.LessThan(3f), "Autopilot is running for 2 seconds");

            var state = AutopilotState.Instance;
            Assert.That(state.IsRunning, Is.False);
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayMode));
            Assert.That(state.exitCode, Is.EqualTo(ExitCode.Normally));
        }

        [Test]
        [PrebuildSetup(typeof(CopyAssetsToResources))]
        [PostBuildCleanup(typeof(CleanupResources))]
        public async Task LaunchAutopilotAsync_WithAssetFile_RunAutopilot()
        {
            var assetPath = CopyAssetsToResources.GetAssetPath("AutopilotSettings2sec.asset"); // lifespanSec is 2

            var beforeTimestamp = Time.time;
            await Launcher.LaunchAutopilotAsync(assetPath);

            var runningTime = Time.time - beforeTimestamp;
            Assert.That(runningTime, Is.GreaterThan(2f).And.LessThan(3f), "Autopilot is running for 2 seconds");

            var state = AutopilotState.Instance;
            Assert.That(state.IsRunning, Is.False);
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayMode));
            Assert.That(state.exitCode, Is.EqualTo(ExitCode.Normally));
        }

        [Test]
        public async Task LaunchAutopilotAsync_InitializeOnLaunchAutopilotMethodWasCalled()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            Assume.That(settings, Is.Not.Null);
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await Launcher.LaunchAutopilotAsync(settings);

            Assert.That(SpyInitializeOnLaunchAutopilot.WasCalled, Is.True);
        }

        [Test]
        public async Task LaunchAutopilotAsync_InitializeOnLaunchAutopilotNonPublicMethodWasCalled()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await Launcher.LaunchAutopilotAsync(settings);

            Assert.That(SpyInitializeOnLaunchAutopilot.WasCalledNonPublicMethod, Is.True);
        }

        [Test]
        public async Task LaunchAutopilotAsync_InitializeOnLaunchAutopilotTaskAsyncWasCalled()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            Assume.That(settings, Is.Not.Null);
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await Launcher.LaunchAutopilotAsync(settings);

            Assert.That(SpyInitializeOnLaunchAutopilot.WasCalledTaskAsync, Is.True);
        }

        [Test]
        public async Task LaunchAutopilotAsync_InitializeOnLaunchAutopilotUniTaskAsyncWasCalled()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            Assume.That(settings, Is.Not.Null);
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await Launcher.LaunchAutopilotAsync(settings);

            Assert.That(SpyInitializeOnLaunchAutopilot.WasCalledUniTaskAsync, Is.True);
        }
    }
}
