// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);
            sut.HandleLog(string.Empty, string.Empty, LogType.Log);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleException = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleException = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleAssert = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleAssert = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleError = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleError = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningHandle_reported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleWarning = true;
            sut.HandleLog(string.Empty, string.Empty, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningNotHandle_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleWarning = false;
            sut.HandleLog(string.Empty, string.Empty, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_ContainsIgnoreMessage_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.ignoreMessages = new[] { "ignore" };
            settings.handleException = true;
            sut.HandleLog("xxx_ignore_xxx", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_StacktraceByLogMessageHandler_notReported()
        {
            var settings = CreateEmptyAutopilotSettings();
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var sut = new LogMessageHandler(settings, spyReporter);

            settings.handleException = true;
            sut.HandleLog(string.Empty, "at DeNA.Anjin.Utilities.LogMessageHandler", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(spyReporter.Arguments, Is.Empty);
        }

        private static AutopilotSettings CreateEmptyAutopilotSettings()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.ignoreMessages = new string[] { };
            return settings;
        }
    }
}
