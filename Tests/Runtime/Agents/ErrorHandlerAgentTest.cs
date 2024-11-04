// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Agents
{
    [TestFixture]
    public class ErrorHandlerAgentTest
    {
        private SpyTerminatable _spyTerminatable;
        private SpyLoggerAsset _spyLoggerAsset;
        private ErrorHandlerAgent _sut;

        private const string Message = "message";
        private const string StackTrace = "stack trace";

        [SetUp]
        public void SetUp()
        {
            _spyTerminatable = new SpyTerminatable();
            _spyLoggerAsset = ScriptableObject.CreateInstance<SpyLoggerAsset>();

            _sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            _sut.Logger = _spyLoggerAsset.Logger;
            _sut._autopilot = _spyTerminatable;
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
            _sut.handleException = true;
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
            _sut.handleException = false;
            _sut.HandleLog(Message, StackTrace, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeAssertHandle_TerminateIsCalled()
        {
            _sut.handleAssert = true;
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
            _sut.handleAssert = false;
            _sut.HandleLog(Message, StackTrace, LogType.Assert);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeErrorHandle_TerminateIsCalled()
        {
            _sut.handleError = true;
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
            _sut.handleError = false;
            _sut.HandleLog(Message, StackTrace, LogType.Error);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_LogTypeWarningHandle_TerminateIsCalled()
        {
            _sut.handleWarning = true;
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
            _sut.handleWarning = false;
            _sut.HandleLog(Message, StackTrace, LogType.Warning);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_ContainsIgnoreMessage_TerminateIsNotCalled()
        {
            _sut.ignoreMessages = new[] { "ignore" };
            _sut.handleException = true;
            _sut.HandleLog("xxx_ignore_xxx", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_MatchIgnoreMessagePattern_TerminateIsNotCalled()
        {
            _sut.ignoreMessages = new[] { "ignore.+ignore" };
            _sut.handleException = true;
            _sut.HandleLog("ignore_xxx_ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_NotMatchIgnoreMessagePattern_TerminateIsCalled()
        {
            _sut.ignoreMessages = new[] { "ignore.+ignore" };
            _sut.handleException = true;
            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
        }

        [Test]
        public async Task HandleLog_InvalidIgnoreMessagePattern_ThrowArgumentExceptionAndTerminateIsCalled()
        {
            _sut.ignoreMessages = new[] { "[a" }; // invalid pattern
            _sut.handleException = true;

            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            LogAssert.Expect(LogType.Exception, "ArgumentException: parsing \"[a\" - Unterminated [] set.");
            Assert.That(_spyTerminatable.IsCalled, Is.True);
        }

        [Test]
        public async Task HandleLog_StacktraceByLogMessageHandler_TerminateIsNotCalled()
        {
            _sut.handleException = true;
            _sut.HandleLog(string.Empty, "at DeNA.Anjin.Agents.ErrorHandlerAgent", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public void OverwriteByCommandLineArguments_NotSpecify_SameAsDefault()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            Assume.That(sut.handleException, Is.True);
            Assume.That(sut.handleError, Is.True);
            Assume.That(sut.handleAssert, Is.True);
            Assume.That(sut.handleWarning, Is.False);

            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(), // Not captured
                _handleError = new StubArgument<bool>(), // Not captured
                _handleAssert = new StubArgument<bool>(), // Not captured
                _handleWarning = new StubArgument<bool>(), // Not captured
            });
            Assert.That(sut.handleException, Is.True);
            Assert.That(sut.handleError, Is.True);
            Assert.That(sut.handleAssert, Is.True);
            Assert.That(sut.handleWarning, Is.False);
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleException_Changed()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(captured: true, value: false), // flip value
                _handleError = new StubArgument<bool>(),
                _handleAssert = new StubArgument<bool>(),
                _handleWarning = new StubArgument<bool>(),
            });
            Assert.That(sut.handleException, Is.False);
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleError_Changed()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(),
                _handleError = new StubArgument<bool>(captured: true, value: false), // flip value
                _handleAssert = new StubArgument<bool>(),
                _handleWarning = new StubArgument<bool>(),
            });
            Assert.That(sut.handleError, Is.False);
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleAssert_Changed()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(),
                _handleError = new StubArgument<bool>(),
                _handleAssert = new StubArgument<bool>(captured: true, value: false), // flip value
                _handleWarning = new StubArgument<bool>(),
            });
            Assert.That(sut.handleAssert, Is.False);
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleWarning_Changed()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(),
                _handleError = new StubArgument<bool>(),
                _handleAssert = new StubArgument<bool>(),
                _handleWarning = new StubArgument<bool>(captured: true, value: true), // flip value
            });
            Assert.That(sut.handleWarning, Is.True);
        }
    }
}
