// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DeNA.Anjin.Editor.UI.Settings;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
#if !UNITY_2020_1_OR_NEWER
using UnityEngine.TestTools;
#endif

namespace DeNA.Anjin
{
#if !UNITY_2020_1_OR_NEWER
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor)] // Fail on Unity 2019 Linux editor
#endif
    [SuppressMessage("ApiDesign", "RS0030")]
    public class LauncherTest
    {
        [SetUp]
        public void SetUp()
        {
            Assume.That(AutopilotState.Instance.IsRunning, Is.False);
        }

        [Test]
        public async Task Launch_InPlayMode_RunAutopilot()
        {
            var testSettings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset");
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(testSettings);
            var state = AutopilotState.Instance;
            editor.Launch(state);

            await Task.Delay(200);

            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.EditorPlayMode));
            Assert.That(state.IsRunning, Is.True, "AutopilotState is running");

            var autopilot = Object.FindObjectOfType<Autopilot>();
            Assert.That((bool)autopilot, Is.True, "Autopilot object is alive");

            await autopilot.TerminateAsync(ExitCode.Normally);
        }

        [Test]
        public async Task Launch_StopAutopilotThatRunInPlayMode_KeepPlayMode()
        {
            var testSettings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset");
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(testSettings);
            var state = AutopilotState.Instance;
            editor.Launch(state);

            await Task.Delay(5000); // 2sec+overhead

            Assert.That(state.IsRunning, Is.False, "AutopilotState is terminated");
            Assert.That(EditorApplication.isPlaying, Is.True, "Keep play mode");
        }

        [Test]
        public async Task Stop_TerminateAutopilotAndKeepPlayMode()
        {
            var testSettings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTestsEndless.asset"); // Expect indefinite execution
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(testSettings);
            var state = AutopilotState.Instance;
            editor.Launch(state);

            await Task.Delay(2000);
            await editor.Stop();
            // Note: If Autopilot stops for life before Stop, a NullReference exception is raised here.

            Assert.That(state.IsRunning, Is.False, "AutopilotState is terminated");
            Assert.That(EditorApplication.isPlaying, Is.True, "Keep play mode");
        }

        [Test]
        public async Task Launch_InitializeOnLaunchAutopilotAttribute_Called()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await LauncherFromTest.AutopilotAsync(settings); // TODO: Renamed in another PR

            Assert.That(SpyInitializeOnLaunchAutopilot.IsCallInitializeOnLaunchAutopilotMethod, Is.True);
        }

        [Test]
        public async Task Launch_InitializeOnLaunchAutopilotAttributeAttachToNonPublicMethod_Called()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await LauncherFromTest.AutopilotAsync(settings); // TODO: Renamed in another PR

            Assert.That(SpyInitializeOnLaunchAutopilot.IsCallInitializeOnLaunchAutopilotMethodNonPublic, Is.True);
        }

        [Test]
        public async Task Launch_InitializeOnLaunchAutopilotAttributeAttachToTaskAsyncMethod_Called()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await LauncherFromTest.AutopilotAsync(settings); // TODO: Renamed in another PR

            Assert.That(SpyInitializeOnLaunchAutopilot.IsCallInitializeOnLaunchAutopilotMethodTaskAsync, Is.True);
        }

        [Test]
        public async Task Launch_InitializeOnLaunchAutopilotAttributeAttachToUniTaskAsyncMethod_Called()
        {
            SpyInitializeOnLaunchAutopilot.Reset();

            var settings = ScriptableObject.CreateInstance(typeof(AutopilotSettings)) as AutopilotSettings;
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.lifespanSec = 1;
            await LauncherFromTest.AutopilotAsync(settings); // TODO: Renamed in another PR

            Assert.That(SpyInitializeOnLaunchAutopilot.IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync, Is.True);
        }
    }
}
