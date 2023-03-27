// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;

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
            sut.slackToken = "token";
            sut.slackChannels = "channel1, channel2, channel3";
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
                _slackToken = new StubArgument<string>(), // Not captured
                _slackChannels = new StubArgument<string>(), // Not captured
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
            Assert.That(sut.slackToken, Is.EqualTo("token"));
            Assert.That(sut.slackChannels, Is.EqualTo("channel1, channel2, channel3"));
        }

        private static Arguments CreateCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(true, 2),
                _randomSeed = new StubArgument<string>(true, ""),
                _timeScale = new StubArgument<float>(true, 2.5f),
                _jUnitReportPath = new StubArgument<string>(true, "/path/to/another_junit_report.xml"),
                _slackToken = new StubArgument<string>(true, "another token"),
                _slackChannels = new StubArgument<string>(true, "channel4"),
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
            Assert.That(sut.slackToken, Is.EqualTo("another token"));
            Assert.That(sut.slackChannels, Is.EqualTo("channel4"));
        }
    }
}
