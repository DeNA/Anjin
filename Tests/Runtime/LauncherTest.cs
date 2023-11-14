// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DeNA.Anjin.Editor.UI.Settings;
using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
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

            Assert.That(state.launchFrom, Is.EqualTo(AutopilotState.LaunchType.EditorPlayMode));
            Assert.That(state.IsRunning, Is.True, "AutopilotState is running");

            var autopilot = Object.FindObjectOfType<Autopilot>();
            Assert.That((bool)autopilot, Is.True, "Autopilot object is alive");

            await autopilot.TerminateAsync(Autopilot.ExitCode.Normally);
        }

        [Test]
        public async Task Launch_StopAutopilotThatRunInPlayMode_KeepPlayMode()
        {
            var testSettings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset");
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(testSettings);
            var state = AutopilotState.Instance;
            editor.Launch(state);

            await Task.Delay(2200); // 2sec+overhead

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
            await editor.Stop(); // Note: If Autopilot stops for life before Stop, a NullReference exception is raised here.

            Assert.That(state.IsRunning, Is.False, "AutopilotState is terminated");
            Assert.That(EditorApplication.isPlaying, Is.True, "Keep play mode");
        }
    }
}
