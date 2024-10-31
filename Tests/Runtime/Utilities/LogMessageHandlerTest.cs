// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Utilities
{
    [TestFixture]
    public class LogMessageHandlerTest
    {
        private AutopilotSettings _settings;
        private SpyReporter _spyReporter;
        private LogMessageHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            _settings.ignoreMessages = new string[] { };
            _spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            _settings.reporter = _spyReporter;
            _sut = new LogMessageHandler(_settings);
        }

        [Test]
        public async Task HandleLog_LogTypeIsLog_notReported()
        {
            _sut.HandleLog(string.Empty, string.Empty, LogType.Log);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionHandle_reported()
        {
            _settings.handleException = true;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionNotHandle_notReported()
        {
            _settings.handleException = false;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertHandle_reported()
        {
            _settings.handleAssert = true;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertNotHandle_notReported()
        {
            _settings.handleAssert = false;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorHandle_reported()
        {
            _settings.handleError = true;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorNotHandle_notReported()
        {
            _settings.handleError = false;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningHandle_reported()
        {
            _settings.handleWarning = true;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningNotHandle_notReported()
        {
            _settings.handleWarning = false;
            _sut.HandleLog(string.Empty, string.Empty, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_ContainsIgnoreMessage_notReported()
        {
            _settings.ignoreMessages = new[] { "ignore" };
            _settings.handleException = true;
            _sut.HandleLog("xxx_ignore_xxx", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_MatchIgnoreMessagePattern_notReported()
        {
            _settings.ignoreMessages = new[] { "ignore.+ignore" };
            _settings.handleException = true;
            _sut.HandleLog("ignore_xxx_ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }

        [Test]
        public async Task HandleLog_NotMatchIgnoreMessagePattern_Reported()
        {
            _settings.ignoreMessages = new[] { "ignore.+ignore" };
            _settings.handleException = true;
            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Not.Empty);
        }

        [Test]
        public async Task HandleLog_InvalidIgnoreMessagePattern_ThrowArgumentExceptionAndReported()
        {
            _settings.ignoreMessages = new[] { "[a" }; // invalid pattern
            _settings.handleException = true;

            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            LogAssert.Expect(LogType.Exception, "ArgumentException: parsing \"[a\" - Unterminated [] set.");
            Assert.That(_spyReporter.Arguments, Is.Not.Empty); // Report is executed
        }

        [Test]
        public async Task HandleLog_StacktraceByLogMessageHandler_notReported()
        {
            _settings.handleException = true;
            _sut.HandleLog(string.Empty, "at DeNA.Anjin.Utilities.LogMessageHandler", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyReporter.Arguments, Is.Empty);
        }
    }
}
