// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading.Tasks;
using DeNA.Anjin.Loggers;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Reporters
{
    [TestFixture]
    public class SlackReporterTest
    {
        private SpySlackMessageSender _spy;
        private SlackReporter _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = ScriptableObject.CreateInstance<SlackReporter>();
            _sut.mentionSubTeamIDs = string.Empty;
            _spy = new SpySlackMessageSender();
            _sut._sender = _spy;

            // for log output test
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.LoggerAsset.loggerAssets.Add(ScriptableObject.CreateInstance<ConsoleLoggerAsset>());
            AutopilotState.Instance.settings = settings;
        }

        [TearDown]
        public void TearDown()
        {
            AutopilotState.Instance.Reset();
        }

        [Test]
        public async Task PostReportAsync_OnError_Sent()
        {
            _sut.slackToken = "token";
            _sut.slackChannels = "dev,qa"; // two channels
            _sut.mentionSubTeamIDs = "alpha,bravo";
            _sut.addHereInSlackMessage = true;
            _sut.messageBodyTemplateOnError = "Error terminate with {message}";
            _sut.messageBodyTemplateOnNormally = "Not use this"; // dummy
            _sut.withScreenshotOnError = false;
            _sut.withScreenshotOnNormally = true; // dummy
            await _sut.PostReportAsync("message", "stack trace", ExitCode.AutopilotFailed);

            Assert.That(_spy.CalledList.Count, Is.EqualTo(2));
            Assert.That(_spy.CalledList[0].SlackToken, Is.EqualTo("token"));
            Assert.That(_spy.CalledList[0].SlackChannel, Is.EqualTo("dev"));
            Assert.That(_spy.CalledList[1].SlackChannel, Is.EqualTo("qa"));
            Assert.That(_spy.CalledList[0].MentionSubTeamIDs, Is.EquivalentTo(new[] { "alpha", "bravo" }));
            Assert.That(_spy.CalledList[0].AddHereInSlackMessage, Is.True);
            Assert.That(_spy.CalledList[0].Message, Is.EqualTo("Error terminate with message")); // use OnNormally
            Assert.That(_spy.CalledList[0].StackTrace, Is.EqualTo("stack trace"));
            Assert.That(_spy.CalledList[0].WithScreenshot, Is.False); // use OnError
        }

        [Test]
        public async Task PostReportAsync_OnNormallyAndPostOnNormally_Sent()
        {
            _sut.slackToken = "token";
            _sut.slackChannels = "dev";
            _sut.postOnNormally = true;
            _sut.mentionSubTeamIDsOnNormally = "charlie";
            _sut.mentionSubTeamIDs = "alpha,bravo"; // dummy
            _sut.addHereInSlackMessageOnNormally = true;
            _sut.addHereInSlackMessage = false; // dummy
            _sut.leadTextOnNormally = "Normally terminate lead";
            _sut.leadTextOnError = "Not use this"; // dummy
            _sut.messageBodyTemplateOnNormally = "Normally terminate with {message}";
            _sut.messageBodyTemplateOnError = "Not use this"; // dummy
            _sut.withScreenshotOnNormally = true;
            _sut.withScreenshotOnError = false; // dummy
            await _sut.PostReportAsync("message", string.Empty, ExitCode.Normally);

            Assert.That(_spy.CalledList.Count, Is.EqualTo(1));
            Assert.That(_spy.CalledList[0].MentionSubTeamIDs, Is.EquivalentTo(new[] { "charlie" }));
            Assert.That(_spy.CalledList[0].AddHereInSlackMessage, Is.True);
            Assert.That(_spy.CalledList[0].Lead, Is.EqualTo("Normally terminate lead")); // use OnNormally
            Assert.That(_spy.CalledList[0].Message, Is.EqualTo("Normally terminate with message")); // use OnNormally
            Assert.That(_spy.CalledList[0].WithScreenshot, Is.True); // use OnNormally
        }

        [Test]
        public async Task PostReportAsync_OnNormallyAndNotPostOnNormally_NotSent()
        {
            _sut.postOnNormally = false;
            await _sut.PostReportAsync(string.Empty, string.Empty, ExitCode.Normally);

            Assert.That(_spy.CalledList, Is.Empty);
        }

        [Test]
        public async Task PostReportAsync_NoSlackToken_NotSentAndLogWarning()
        {
            _sut.slackToken = string.Empty;
            _sut.slackChannels = "dev";
            await _sut.PostReportAsync(string.Empty, string.Empty, ExitCode.AutopilotFailed);

            Assert.That(_spy.CalledList, Is.Empty);
            LogAssert.Expect(LogType.Warning, "Slack token or channels is empty");
        }

        [Test]
        public async Task PostReportAsync_NoSlackChannels_NotSentAndLogWarning()
        {
            _sut.slackToken = "token";
            _sut.slackChannels = string.Empty;
            await _sut.PostReportAsync(string.Empty, string.Empty, ExitCode.AutopilotFailed);

            Assert.That(_spy.CalledList, Is.Empty);
            LogAssert.Expect(LogType.Warning, "Slack token or channels is empty");
        }
    }
}
