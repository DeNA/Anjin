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
        private SpyTerminatable _spyTerminatable;
        private LogMessageHandler _sut;

        private const string Message = "message";
        private const string StackTrace = "stack trace";

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            _settings.lifespanSec = 5;
            _settings.ignoreMessages = new string[] { };

            _spyTerminatable = new SpyTerminatable();
            _sut = new LogMessageHandler(_settings, _spyTerminatable);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task HandleLog_LogTypeIsLog_TerminateIsNotCalled()
        {
            _sut.HandleLog(Message, StackTrace, LogType.Log);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionHandle_TerminateIsCalled()
        {
            _settings.handleException = true;
            _sut.HandleLog(Message, StackTrace, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
            Assert.That(_spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.UnCatchExceptions));
            Assert.That(_spyTerminatable.CapturedMessage, Is.EqualTo(Message));
            Assert.That(_spyTerminatable.CapturedStackTrace, Is.EqualTo(StackTrace));
            Assert.That(_spyTerminatable.CapturedReporting, Is.True);
        }

        [Test]
        public async Task HandleLog_LogTypeExceptionNotHandle_TerminateIsNotCalled()
        {
            _settings.handleException = false;
            _sut.HandleLog(Message, StackTrace, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertHandle_TerminateIsCalled()
        {
            _settings.handleAssert = true;
            _sut.HandleLog(Message, StackTrace, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
            Assert.That(_spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
            Assert.That(_spyTerminatable.CapturedMessage, Is.EqualTo(Message));
            Assert.That(_spyTerminatable.CapturedStackTrace, Is.EqualTo(StackTrace));
            Assert.That(_spyTerminatable.CapturedReporting, Is.True);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertNotHandle_TerminateIsNotCalled()
        {
            _settings.handleAssert = false;
            _sut.HandleLog(Message, StackTrace, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorHandle_TerminateIsCalled()
        {
            _settings.handleError = true;
            _sut.HandleLog(Message, StackTrace, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
            Assert.That(_spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
            Assert.That(_spyTerminatable.CapturedMessage, Is.EqualTo(Message));
            Assert.That(_spyTerminatable.CapturedStackTrace, Is.EqualTo(StackTrace));
            Assert.That(_spyTerminatable.CapturedReporting, Is.True);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorNotHandle_TerminateIsNotCalled()
        {
            _settings.handleError = false;
            _sut.HandleLog(Message, StackTrace, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningHandle_TerminateIsCalled()
        {
            _settings.handleWarning = true;
            _sut.HandleLog(Message, StackTrace, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
            Assert.That(_spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
            Assert.That(_spyTerminatable.CapturedMessage, Is.EqualTo(Message));
            Assert.That(_spyTerminatable.CapturedStackTrace, Is.EqualTo(StackTrace));
            Assert.That(_spyTerminatable.CapturedReporting, Is.True);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningNotHandle_TerminateIsNotCalled()
        {
            _settings.handleWarning = false;
            _sut.HandleLog(Message, StackTrace, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_ContainsIgnoreMessage_TerminateIsNotCalled()
        {
            _settings.ignoreMessages = new[] { "ignore" };
            _settings.handleException = true;
            _sut.HandleLog("xxx_ignore_xxx", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_MatchIgnoreMessagePattern_TerminateIsNotCalled()
        {
            _settings.ignoreMessages = new[] { "ignore.+ignore" };
            _settings.handleException = true;
            _sut.HandleLog("ignore_xxx_ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_NotMatchIgnoreMessagePattern_TerminateIsCalled()
        {
            _settings.ignoreMessages = new[] { "ignore.+ignore" };
            _settings.handleException = true;
            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
        }

        [Test]
        public async Task HandleLog_InvalidIgnoreMessagePattern_ThrowArgumentExceptionAndTerminateIsCalled()
        {
            _settings.ignoreMessages = new[] { "[a" }; // invalid pattern
            _settings.handleException = true;

            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            LogAssert.Expect(LogType.Exception, "ArgumentException: parsing \"[a\" - Unterminated [] set.");
            Assert.That(_spyTerminatable.IsCalled, Is.True);
        }

        [Test]
        public async Task HandleLog_StacktraceByLogMessageHandler_TerminateIsNotCalled()
        {
            _settings.handleException = true;
            _sut.HandleLog(string.Empty, "at DeNA.Anjin.Utilities.LogMessageHandler", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }
    }
}
