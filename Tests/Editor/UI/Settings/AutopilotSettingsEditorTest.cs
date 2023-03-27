// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using DeNA.Anjin.Editor.Fakes;
using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Editor.UI.Settings
{
    /// <summary>
    /// Test case about AutopilotSettingsEditor and starting Autopilot from edit mode.
    /// Start/Stop from play mode tests are implemented in Runtime/LauncherTest.cs
    /// </summary>
    [TestFixture]
    [SuppressMessage("ApiDesign", "RS0030")]
    public class AutopilotSettingsEditorTest
    {
        /// <summary>
        /// Detect disabled domain reloading. Because test will not work as intended.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            if (EditorSettings.enterPlayModeOptionsEnabled)
            {
                Assume.That(EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload),
                    Is.False, "Enabled domain reloading option");
            }
            else
            {
                Assume.That(EditorSettings.enterPlayModeOptionsEnabled, Is.False, "Enabled domain reloading");
            }
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            if (EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();
                // Impossible waiting until stop in edit mode tests. so switch to edit mode and exit.
            }
        }

        [UnityTest]
        public IEnumerator Launch_RunAutopilotOnPlayMode()
        {
            var testSettings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset");
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(testSettings);
            var state = AutopilotState.Instance;
            editor.Launch(state); // Note: Can not call editor.OnInspectorGUI() and GUILayout.Button()

            yield return new WaitForDomainReload(); // Wait for domain reloading by switching play mode

            state = AutopilotState.Instance; // Reacquire because lost in domain reloading
            Assert.That(state.IsRunning, Is.True, "AutopilotState is running");

            var autopilot = Object.FindObjectOfType<Autopilot>();
            Assert.That((bool)autopilot, Is.True, "Autopilot object is alive");
        }

        [UnityTest]
        public IEnumerator Launch_RunAutopilot_CallMethodWithInitializeOnLaunchAutopilotAttribute()
            // Note: This test about InitializeOnLaunchAutopilotAttribute
        {
            FakeInitializeOnLaunchAutopilot.Reset();

            var testSettings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                "Packages/com.dena.anjin/Tests/TestAssets/AutopilotSettingsForTests.asset");
            var editor = (AutopilotSettingsEditor)UnityEditor.Editor.CreateEditor(testSettings);
            var state = AutopilotState.Instance;
            editor.Launch(state); // Note: Can not call editor.OnInspectorGUI() and GUILayout.Button()

            yield return new WaitForDomainReload(); // Wait for domain reloading by switching play mode

            Assert.That(FakeInitializeOnLaunchAutopilot.IsCallInitializeOnLaunchAutopilotMethod, Is.True);
        }
    }
}
