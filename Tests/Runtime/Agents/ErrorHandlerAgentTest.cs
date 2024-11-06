// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
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
        private SpyReporter _spyReporter;
        private ErrorHandlerAgent _sut;

        private const string Message = "MESSAGE";
        private const string StackTrace = "STACK TRACE";

        [SetUp]
        public void SetUp()
        {
            _spyTerminatable = new SpyTerminatable();
            _spyLoggerAsset = ScriptableObject.CreateInstance<SpyLoggerAsset>();

            _sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            _sut._autopilot = _spyTerminatable;
            _sut.Logger = _spyLoggerAsset.Logger;

            _spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.Reporter.reporters.Add(_spyReporter);
            AutopilotState.Instance.settings = settings;
        }

        [TearDown]
        public void TearDown()
        {
            AutopilotState.Instance.Reset();
        }

        [Test]
        public async Task HandleLog_LogTypeLog_NotCallTerminate()
        {
            _sut.HandleLog(Message, StackTrace, LogType.Log);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        private static IEnumerable<TestCaseData> LogTypeTestCase()
        {
            yield return new TestCaseData(LogType.Exception, ExitCode.UnCatchExceptions);
            yield return new TestCaseData(LogType.Error, ExitCode.AutopilotFailed);
            yield return new TestCaseData(LogType.Assert, ExitCode.AutopilotFailed);
            yield return new TestCaseData(LogType.Warning, ExitCode.AutopilotFailed);
        }

        private void SetHandlingBehavior(LogType logType, HandlingBehavior handlingBehavior)
        {
            _sut.handleException = HandlingBehavior.Ignore;
            _sut.handleError = HandlingBehavior.Ignore;
            _sut.handleAssert = HandlingBehavior.Ignore;
            _sut.handleWarning = HandlingBehavior.Ignore;

            switch (logType)
            {
                case LogType.Exception:
                    _sut.handleException = handlingBehavior;
                    break;
                case LogType.Error:
                    _sut.handleError = handlingBehavior;
                    break;
                case LogType.Assert:
                    _sut.handleAssert = handlingBehavior;
                    break;
                case LogType.Warning:
                    _sut.handleWarning = handlingBehavior;
                    break;
                case LogType.Log:
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        [TestCaseSource(nameof(LogTypeTestCase))]
        public async Task HandleLog_HandlingBehaviorIsIgnore(LogType logType, ExitCode _)
        {
            SetHandlingBehavior(logType, HandlingBehavior.Ignore);
            _sut.HandleLog(Message, StackTrace, logType);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
            Assert.That(_spyReporter.IsCalled, Is.False);
        }

        [TestCaseSource(nameof(LogTypeTestCase))]
        public async Task HandleLog_HandlingBehaviorIsReportOnly(LogType logType, ExitCode expectedExitCode)
        {
            SetHandlingBehavior(logType, HandlingBehavior.ReportOnly);
            _sut.HandleLog(Message, StackTrace, logType);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
            Assert.That(_spyReporter.IsCalled, Is.True);
            Assert.That(_spyReporter.Arguments["message"], Is.EqualTo(Message));
            Assert.That(_spyReporter.Arguments["stackTrace"], Is.EqualTo(StackTrace));
            Assert.That(_spyReporter.Arguments["exitCode"], Is.EqualTo(expectedExitCode.ToString()));
        }

        [TestCaseSource(nameof(LogTypeTestCase))]
        public async Task HandleLog_HandlingBehaviorIsTerminateAutopilot(LogType logType, ExitCode expectedExitCode)
        {
            SetHandlingBehavior(logType, HandlingBehavior.TerminateAutopilot);
            _sut.HandleLog(Message, StackTrace, logType);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
            Assert.That(_spyTerminatable.CapturedExitCode, Is.EqualTo(expectedExitCode));
            Assert.That(_spyTerminatable.CapturedMessage, Is.EqualTo(Message));
            Assert.That(_spyTerminatable.CapturedStackTrace, Is.EqualTo(StackTrace));
            Assert.That(_spyTerminatable.CapturedReporting, Is.True);
        }

        [Test]
        public async Task HandleLog_ContainsIgnoreMessage_NotTerminate()
        {
            _sut.ignoreMessages = new[] { "ignore" };
            _sut.handleException = HandlingBehavior.TerminateAutopilot;
            _sut.HandleLog("xxx_ignore_xxx", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_MatchIgnoreMessagePattern_NotTerminate()
        {
            _sut.ignoreMessages = new[] { "ignore.+ignore" };
            _sut.handleException = HandlingBehavior.TerminateAutopilot;
            _sut.HandleLog("ignore_xxx_ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public async Task HandleLog_NotMatchIgnoreMessagePattern_Terminate()
        {
            _sut.ignoreMessages = new[] { "ignore.+ignore" };
            _sut.handleException = HandlingBehavior.TerminateAutopilot;
            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.True);
        }

        [Test]
        public async Task HandleLog_InvalidIgnoreMessagePattern_ThrowArgumentExceptionAndTerminate()
        {
            _sut.ignoreMessages = new[] { "[a" }; // invalid pattern
            _sut.handleException = HandlingBehavior.TerminateAutopilot;

            _sut.HandleLog("ignore", string.Empty, LogType.Exception);
            await UniTask.NextFrame();

            LogAssert.Expect(LogType.Exception, "ArgumentException: parsing \"[a\" - Unterminated [] set.");
            Assert.That(_spyTerminatable.IsCalled, Is.True);
        }

        [Test]
        public async Task HandleLog_StackTraceContainsErrorHandlerAgent_NotTerminate()
        {
            _sut.handleException = HandlingBehavior.TerminateAutopilot;
            _sut.HandleLog(string.Empty, "at DeNA.Anjin.Agents.ErrorHandlerAgent", LogType.Exception);
            await UniTask.NextFrame();

            Assert.That(_spyTerminatable.IsCalled, Is.False);
        }

        [Test]
        public void OverwriteByCommandLineArguments_NotSpecified_SameAsDefault()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            Assume.That(sut.handleException, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
            Assume.That(sut.handleError, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
            Assume.That(sut.handleAssert, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
            Assume.That(sut.handleWarning, Is.EqualTo(HandlingBehavior.Ignore));

            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(), // Not captured
                _handleError = new StubArgument<bool>(), // Not captured
                _handleAssert = new StubArgument<bool>(), // Not captured
                _handleWarning = new StubArgument<bool>(), // Not captured
            });
            Assert.That(sut.handleException, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
            Assert.That(sut.handleError, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
            Assert.That(sut.handleAssert, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
            Assert.That(sut.handleWarning, Is.EqualTo(HandlingBehavior.Ignore));
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleException_Overwrite()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(captured: true, value: false), // flip value
                _handleError = new StubArgument<bool>(),
                _handleAssert = new StubArgument<bool>(),
                _handleWarning = new StubArgument<bool>(),
            });
            Assert.That(sut.handleException, Is.EqualTo(HandlingBehavior.Ignore));
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleError_Overwrite()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(),
                _handleError = new StubArgument<bool>(captured: true, value: false), // flip value
                _handleAssert = new StubArgument<bool>(),
                _handleWarning = new StubArgument<bool>(),
            });
            Assert.That(sut.handleError, Is.EqualTo(HandlingBehavior.Ignore));
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleAssert_Overwrite()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(),
                _handleError = new StubArgument<bool>(),
                _handleAssert = new StubArgument<bool>(captured: true, value: false), // flip value
                _handleWarning = new StubArgument<bool>(),
            });
            Assert.That(sut.handleAssert, Is.EqualTo(HandlingBehavior.Ignore));
        }

        [Test]
        public void OverwriteByCommandLineArguments_SpecifyHandleWarning_Overwrite()
        {
            var sut = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            sut.OverwriteByCommandLineArguments(new StubArguments()
            {
                _handleException = new StubArgument<bool>(),
                _handleError = new StubArgument<bool>(),
                _handleAssert = new StubArgument<bool>(),
                _handleWarning = new StubArgument<bool>(captured: true, value: true), // flip value
            });
            Assert.That(sut.handleWarning, Is.EqualTo(HandlingBehavior.TerminateAutopilot));
        }
    }
}
