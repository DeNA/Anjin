// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Reporters
{
    public class MessageBuilderTest
    {
        [Test]
        public void BuildWithTemplate_Message_ReplacesMessage()
        {
            var actual = MessageBuilder.BuildWithTemplate("Error: {message}.", "Something went wrong");
            Assert.That(actual, Is.EqualTo("Error: Something went wrong."));
        }

        [Test]
        public void BuildWithTemplate_Settings_ReplacesSettingsName()
        {
            AutopilotState.Instance.settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            AutopilotState.Instance.settings!.name = "Settings for test";

            var actual = MessageBuilder.BuildWithTemplate("Autopilot settings: {settings}.", null);
            Assert.That(actual, Is.EqualTo("Autopilot settings: Settings for test."));

            // teardown
            AutopilotState.Instance.Reset();
        }

        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer })]
        public void BuildWithTemplate_EnvKey_ReplacesEnvironmentVariable()
        {
            var actual = MessageBuilder.BuildWithTemplate("Environment variable SHELL: {env.SHELL}.", null);
            Assert.That(actual, Is.EqualTo("Environment variable SHELL: /bin/bash."));
        }
    }
}
