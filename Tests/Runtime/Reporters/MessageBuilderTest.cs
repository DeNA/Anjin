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
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.name = "Settings for test";
            AutopilotState.Instance.settings = settings;

            var actual = MessageBuilder.BuildWithTemplate("Autopilot settings: {settings}.", null);
            Assert.That(actual, Is.EqualTo("Autopilot settings: Settings for test."));

            // teardown
            AutopilotState.Instance.Reset();
        }

        [Test]
        [IgnoreWindowMode("This test requires environment variables. see Makefile")]
        [Category("IgnoreCI")] // This test requires environment variables.
        public void BuildWithTemplate_EnvKey_ReplacesEnvironmentVariable()
        {
            var actual = MessageBuilder.BuildWithTemplate("Environment variable STR_ENV: {env.STR_ENV}.", null);
            Assert.That(actual, Is.EqualTo("Environment variable STR_ENV: STRING_BY_ENVIRONMENT_VARIABLE."));
        }
    }
}
