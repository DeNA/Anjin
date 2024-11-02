// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Settings;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;

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
        [IgnoreWindowMode("Need command line arguments. see Makefile")]
        public void BuildWithTemplate_EnvKey_ReplacesEnvironmentVariable()
        {
            var actual = MessageBuilder.BuildWithTemplate("Environment variable STR_ENV: {env.STR_ENV}.", null);
            Assert.That(actual, Is.EqualTo("Environment variable STR_ENV: STRING_BY_ENVIRONMENT_VARIABLE."));
        }
    }
}
