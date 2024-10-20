// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin
{
    [TestFixture]
    public class AutopilotTest
    {
        [Test]
        public async Task Start_LoggerIsNotSet_UsingDefaultLogger()
        {
            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.sceneAgentMaps = new List<SceneAgentMap>();
            autopilotSettings.lifespanSec = 1;

            await LauncherFromTest.AutopilotAsync(autopilotSettings);

            LogAssert.Expect(LogType.Log, "Launched autopilot"); // using console logger
        }

        [Test]
        public async Task Start_LoggerSpecified_UsingSpecifiedLogger()
        {
            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.sceneAgentMaps = new List<SceneAgentMap>();
            autopilotSettings.lifespanSec = 1;
            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            autopilotSettings.loggerAsset = spyLogger;

            await LauncherFromTest.AutopilotAsync(autopilotSettings);

            Assert.That(spyLogger.Logs, Does.Contain("Launched autopilot")); // using spy logger
            LogAssert.NoUnexpectedReceived(); // not write to console
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasSlackSettings_GenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "token";
            settings.slackChannels = "channels";
            settings.mentionSubTeamIDs = "subteam";
            settings.addHereInSlackMessage = true;

            Autopilot.ConvertSlackReporterFromObsoleteSlackSettings(settings, Debug.unityLogger);

            var slackReporter = settings.reporter as SlackReporter;
            Assert.That(slackReporter, Is.Not.Null);
            Assert.That(slackReporter.slackToken, Is.EqualTo(settings.slackToken));
            Assert.That(slackReporter.slackChannels, Is.EqualTo(settings.slackChannels));
            Assert.That(slackReporter.mentionSubTeamIDs, Is.EqualTo(settings.mentionSubTeamIDs));
            Assert.That(slackReporter.addHereInSlackMessage, Is.EqualTo(settings.addHereInSlackMessage));

            LogAssert.Expect(LogType.Warning, @"Slack settings in AutopilotSettings has been obsoleted.
Please delete the value using Debug Mode in the Inspector window. And create a SlackReporter asset file.
This time, temporarily generate and use SlackReporter instance.");
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasNotSlackSettings_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "token";

            Autopilot.ConvertSlackReporterFromObsoleteSlackSettings(settings, Debug.unityLogger);
            Assert.That(settings.reporter, Is.Null);
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasSlackReporter_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.reporter = ScriptableObject.CreateInstance<CompositeReporter>();
            settings.slackToken = "token";
            settings.slackChannels = "channels";

            Autopilot.ConvertSlackReporterFromObsoleteSlackSettings(settings, Debug.unityLogger);
            Assert.That(settings.reporter, Is.InstanceOf<CompositeReporter>());
        }
    }
}
