// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections;
using DeNA.Anjin.Editor.YieldInstructions;
using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Editor.UI.Settings
{
    /// <summary>
    /// Test case about AutopilotSettingsEditor and starting Autopilot from edit mode.
    /// Start/Stop from play mode tests are implemented in Runtime/LauncherTest.cs
    /// </summary>
    /// <remarks>
    /// Please run these tests on two settings:
    ///   1. Enable domain reloading (Turn off "Project Settings | Editor | Enter Play Mode Options")
    ///   2. Disable domain reloading (Turn on "Project Settings | Editor | Enter Play Mode Options" and turn off "Reload Domain")
    /// TODO: Change to parameterized tests. However, it requires saving "Enter Play Mode Settings".
    /// </remarks>
    [TestFixture]
    [Timeout(15000)]
    public class AutopilotSettingsEditorTest
    {
        private static AutopilotSettings LoadAutopilotSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset"); // lifespanSec is 2
            return settings;
        }

        [UnityTest]
        public IEnumerator Launch_InEditMode_RunAutopilot()
        {
            var settings = LoadAutopilotSettings();
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(settings);
            editor.Launch(); // Note: Can not call editor.OnInspectorGUI() and GUILayout.Button()
            yield return new WaitForAutopilotToRun();

            var state = AutopilotState.Instance;
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.EditMode));
            Assert.That(state.IsRunning, Is.True);

            // Tear down
            settings = LoadAutopilotSettings(); // After reloading domain, the instances are lost. So, re-create it.
            editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(settings);
            editor.Stop();
            yield return new WaitForAutopilotToTerminate();
        }

        [UnityTest]
        public IEnumerator Launch_InPlayMode_RunAutopilot()
        {
            yield return new EnterPlayMode();

            var settings = LoadAutopilotSettings();
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(settings);
            editor.Launch(); // Note: Can not call editor.OnInspectorGUI() and GUILayout.Button()
            yield return new WaitForAutopilotToRun();

            var state = AutopilotState.Instance;
            Assert.That(state.launchFrom, Is.EqualTo(LaunchType.PlayMode));
            Assert.That(state.IsRunning, Is.True);

            // Tear down
            editor.Stop();
            yield return new WaitForAutopilotToTerminate();
        }

        [UnityTest]
        public IEnumerator Stop_InEditMode_TerminateAutopilotAndExitPlayMode()
        {
            var settings = LoadAutopilotSettings();
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(settings);
            editor.Launch(); // Note: Can not call editor.OnInspectorGUI() and GUILayout.Button()
            yield return new WaitForAutopilotToRun();

            settings = LoadAutopilotSettings(); // After reloading domain, the instances are lost. So, re-create it.
            editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(settings);
            editor.Stop();
            yield return new WaitForAutopilotToTerminate();

            var state = AutopilotState.Instance;
            Assume.That(state.IsRunning, Is.False, "Running autopilot");
            Assert.That(EditorApplication.isPlaying, Is.False, "Keep playmode");
        }

        [UnityTest]
        public IEnumerator Stop_InPlayMode_TerminateAutopilotAndKeepPlayMode()
        {
            yield return new EnterPlayMode();

            var settings = LoadAutopilotSettings();
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(settings);
            editor.Launch(); // Note: Can not call editor.OnInspectorGUI() and GUILayout.Button()
            yield return new WaitForAutopilotToRun();

            editor.Stop();
            yield return new WaitForAutopilotToTerminate();

            var state = AutopilotState.Instance;
            Assume.That(state.IsRunning, Is.False, "Running autopilot");
            Assert.That(EditorApplication.isPlaying, Is.True, "Keep playmode");
        }
    }
}
