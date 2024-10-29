// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Reporters;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Settings
{
    public class AutopilotSettingsTest
    {
        private static AutopilotSettings CreateAutopilotSettings()
        {
            var sut = ScriptableObject.CreateInstance<AutopilotSettings>();
            sut.lifespanSec = 5;
            sut.randomSeed = "1";
            sut.timeScale = 1.0f;
            sut.junitReportPath = "path/to/junit_report.xml";
            sut.handleException = true;
            sut.handleError = true;
            sut.handleAssert = true;
            sut.handleWarning = true;
            return sut;
        }

        private static Arguments CreateNotCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(), // Not captured
                _randomSeed = new StubArgument<string>(), // Not captured
                _timeScale = new StubArgument<float>(), // Not captured
                _jUnitReportPath = new StubArgument<string>(), // Not captured
                _handleException = new StubArgument<bool>(), // Not captured
                _handleError = new StubArgument<bool>(), // Not captured
                _handleAssert = new StubArgument<bool>(), // Not captured
                _handleWarning = new StubArgument<bool>(), // Not captured
            };
            return arguments;
        }

        [Test]
        public void OverrideByCommandLineArguments_HasNotCommandlineArguments_KeepScriptableObjectValues()
        {
            var sut = CreateAutopilotSettings();
            sut.OverrideByCommandLineArguments(CreateNotCapturedArguments());

            Assert.That(sut.lifespanSec, Is.EqualTo(5));
            Assert.That(sut.randomSeed, Is.EqualTo("1"));
            Assert.That(sut.timeScale, Is.EqualTo(1.0f));
            Assert.That(sut.junitReportPath, Is.EqualTo("path/to/junit_report.xml"));
            Assert.That(sut.handleException, Is.True);
            Assert.That(sut.handleError, Is.True);
            Assert.That(sut.handleAssert, Is.True);
            Assert.That(sut.handleWarning, Is.True);
        }

        private static Arguments CreateCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(true, 2),
                _randomSeed = new StubArgument<string>(true, ""),
                _timeScale = new StubArgument<float>(true, 2.5f),
                _jUnitReportPath = new StubArgument<string>(true, "/path/to/another_junit_report.xml"),
                _handleException = new StubArgument<bool>(true, false),
                _handleError = new StubArgument<bool>(true, false),
                _handleAssert = new StubArgument<bool>(true, false),
                _handleWarning = new StubArgument<bool>(true, false),
            };
            return arguments;
        }

        [Test]
        public void OverrideByCommandLineArguments_HasCommandlineArguments_OverwriteValues()
        {
            var sut = CreateAutopilotSettings();
            sut.OverrideByCommandLineArguments(CreateCapturedArguments());

            Assert.That(sut.lifespanSec, Is.EqualTo(2));
            Assert.That(sut.randomSeed, Is.EqualTo(""));
            Assert.That(sut.timeScale, Is.EqualTo(2.5f));
            Assert.That(sut.junitReportPath, Is.EqualTo("/path/to/another_junit_report.xml"));
            Assert.That(sut.handleException, Is.False);
            Assert.That(sut.handleError, Is.False);
            Assert.That(sut.handleAssert, Is.False);
            Assert.That(sut.handleWarning, Is.False);
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasSlackSettings_GenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "token";
            settings.slackChannels = "channels";
            settings.mentionSubTeamIDs = "subteam";
            settings.addHereInSlackMessage = true;

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);

            Assert.That(settings.reporters.Count, Is.EqualTo(1));
            var slackReporter = settings.reporters[0] as SlackReporter;
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
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasNotSlackToken_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "token";

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);
            Assert.That(settings.reporters, Is.Empty);
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasNotSlackChannels_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackChannels = "channels";

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);
            Assert.That(settings.reporters, Is.Empty);
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_ExistReporter_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.reporters.Add(ScriptableObject.CreateInstance<CompositeReporter>()); // already exists
            settings.slackToken = "token";
            settings.slackChannels = "channels";

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);
            Assert.That(settings.reporters.Count, Is.EqualTo(1));
            Assert.That(settings.reporters, Has.No.InstanceOf<SlackReporter>());
        }
    }
}
