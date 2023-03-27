// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Utilities
{
    [TestFixture]
    public class LogMessageHandlerTest
    {
        [Test]
        public async Task HandleLog_LogTypeIsLog_notReported()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);
            sut.HandleLog(string.Empty, string.Empty, LogType.Log);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleException = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleException = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleAssert = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleAssert = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleError = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleError = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleWarning = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleWarning = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_ContainsIgnoreMessage_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.ignoreMessages = new[] { "ignore" };
            settings.handleException = true;
            sut.HandleLog("xxx_ignore_xxx", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_StacktraceByLogMessageHandler_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleException = true;
            sut.HandleLog(string.Empty, "at DeNA.Anjin.Utilities.LogMessageHandler", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_StacktraceBySlackAPI_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.handleException = true;
            sut.HandleLog(string.Empty, "at DeNA.Anjin.Reporters.SlackAPI", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spySlackAPI.Arguments, Is.Empty);
        }

        [Test]
        [Category("IgnoreCI")] // Reason: screen capture is not work run on batchmode
        public async Task HandleLog_SingleChannel_PostToSlack()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.slackToken = "TOKEN";
            settings.slackChannels = "CHANNEL";
            settings.mentionSubTeamIDs = string.Empty;
            settings.addHereInSlackMessage = false;
            settings.handleException = true;
            sut.HandleLog("MESSAGE", "STACKTRACE", LogType.Exception);
            await UniTask.DelayFrame(5);

            var expected1 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL" }, { "message", "MESSAGE" }, { "ts", null }
            };
            var expected2 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL" }, { "message", "IMAGE" }, { "ts", "1" }
            };
            var expected3 = new Dictionary<string, string>
            {
                { "token", "TOKEN" },
                { "channel", "CHANNEL" },
                { "message", "MESSAGE\n\n```STACKTRACE```" },
                { "ts", "1" }
            };
            var expected = new List<Dictionary<string, string>> { expected1, expected2, expected3 };
            var actual = spySlackAPI.Arguments;

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public async Task HandleLog_SingleChannelWithMention_PostToSlack()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.slackToken = "TOKEN";
            settings.slackChannels = "CHANNEL";
            settings.mentionSubTeamIDs = "MENTION1,MENTION2";
            settings.addHereInSlackMessage = false;
            settings.handleException = true;
            sut.HandleLog("MESSAGE", "STACKTRACE", LogType.Exception);
            await UniTask.DelayFrame(5);

            var expected1 = new Dictionary<string, string>
            {
                { "token", "TOKEN" },
                { "channel", "CHANNEL" },
                { "message", "<!subteam^MENTION1> <!subteam^MENTION2> MESSAGE" },
                { "ts", null }
            };
            var actual = spySlackAPI.Arguments;

            Assert.That(actual[0], Is.EqualTo(expected1));
        }

        [Test]
        public async Task HandleLog_SingleChannelWithHere_PostToSlack()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.slackToken = "TOKEN";
            settings.slackChannels = "CHANNEL";
            settings.mentionSubTeamIDs = string.Empty;
            settings.addHereInSlackMessage = true; // add `@here`
            settings.handleException = true;
            sut.HandleLog("MESSAGE", "STACKTRACE", LogType.Exception);
            await UniTask.DelayFrame(5);

            var expected1 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL" }, { "message", "<!here> MESSAGE" }, { "ts", null }
            };
            var actual = spySlackAPI.Arguments;

            Assert.That(actual[0], Is.EqualTo(expected1));
        }

        [Test]
        [Category("IgnoreCI")] // Reason: screen capture is not work run on batchmode
        public async Task HandleLog_MultiChannel_PostToSlack()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spySlackAPI = new SpySlackAPI();
            var reporter = new SlackReporter(settings, spySlackAPI);
            var sut = new LogMessageHandler(settings, reporter);

            settings.slackToken = "TOKEN";
            settings.slackChannels = "CHANNEL1,CHANNEL2";
            settings.mentionSubTeamIDs = string.Empty;
            settings.addHereInSlackMessage = false;
            settings.handleException = true;
            sut.HandleLog("MESSAGE", "STACKTRACE", LogType.Exception);
            await UniTask.DelayFrame(8);

            var expected1 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL1" }, { "message", "MESSAGE" }, { "ts", null }
            };
            var expected2 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL1" }, { "message", "IMAGE" }, { "ts", "1" }
            };
            var expected3 = new Dictionary<string, string>
            {
                { "token", "TOKEN" },
                { "channel", "CHANNEL1" },
                { "message", "MESSAGE\n\n```STACKTRACE```" },
                { "ts", "1" }
            };
            var expected4 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL2" }, { "message", "MESSAGE" }, { "ts", null }
            };
            var expected5 = new Dictionary<string, string>
            {
                { "token", "TOKEN" }, { "channel", "CHANNEL2" }, { "message", "IMAGE" }, { "ts", "1" }
            };
            var expected6 = new Dictionary<string, string>
            {
                { "token", "TOKEN" },
                { "channel", "CHANNEL2" },
                { "message", "MESSAGE\n\n```STACKTRACE```" },
                { "ts", "1" }
            };
            var expected = new List<Dictionary<string, string>>
            {
                expected1,
                expected2,
                expected3,
                expected4,
                expected5,
                expected6
            };
            var actual = spySlackAPI.Arguments;

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static AutopilotSettings CreateEmptyAutopilotSettings()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "dummy token";
            settings.slackChannels = "dummy channel";
            settings.mentionSubTeamIDs = string.Empty;
            settings.ignoreMessages = new string[] { };
            return settings;
        }
    }
}
