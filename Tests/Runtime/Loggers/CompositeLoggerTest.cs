// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class CompositeLoggerTest
    {
        private static Exception CreateExceptionWithStacktrace(string message)
        {
            try
            {
                throw new ApplicationException(message);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        [Test]
        public void LogFormat_CompositeMultipleLoggers_OutputBothLoggers()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger1 = ScriptableObject.CreateInstance<SpyLogger>();
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(logger1);
            sut.loggers.Add(logger2);

            sut.LoggerImpl.Log(message);

            Assert.That(logger1.Logs, Does.Contain(message));
            Assert.That(logger2.Logs, Does.Contain(message));
        }

        [Test]
        public void LogFormat_LoggersIncludesNull_IgnoreNull()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(null);
            sut.loggers.Add(logger2);

            sut.LoggerImpl.Log(message);

            Assert.That(logger2.Logs, Does.Contain(message));
        }

        [Test]
        public void LogFormat_NestingLogger_IgnoreNested()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(sut); // nesting
            sut.loggers.Add(logger2);

            sut.LoggerImpl.Log(message);

            Assert.That(logger2.Logs, Does.Contain(message));
        }

        [Test]
        public void LogException_CompositeMultipleLoggers_OutputBothLoggers()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger1 = ScriptableObject.CreateInstance<SpyLogger>();
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(logger1);
            sut.loggers.Add(logger2);

            var exception = CreateExceptionWithStacktrace(message);
            sut.LoggerImpl.LogException(exception);

            Assert.That(logger1.Logs, Does.Contain(exception.ToString()));
            Assert.That(logger2.Logs, Does.Contain(exception.ToString()));
        }

        [Test]
        public void LogException_LoggersIncludesNull_IgnoreNull()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(null);
            sut.loggers.Add(logger2);

            var exception = CreateExceptionWithStacktrace(message);
            sut.LoggerImpl.LogException(exception);

            Assert.That(logger2.Logs, Does.Contain(exception.ToString()));
        }

        [Test]
        public void LogException_NestingLogger_IgnoreNested()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(sut); // nesting
            sut.loggers.Add(logger2);

            var exception = CreateExceptionWithStacktrace(message);
            sut.LoggerImpl.LogException(exception);

            Assert.That(logger2.Logs, Does.Contain(exception.ToString()));
        }

        [Test]
        public void Dispose_CompositeMultipleLoggers_DisposeAllLoggers()
        {
            var logger1 = ScriptableObject.CreateInstance<SpyLogger>();
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(logger1);
            sut.loggers.Add(logger2);

            sut.Dispose();

            Assert.That(logger1.Disposed, Is.True);
            Assert.That(logger2.Disposed, Is.True);
        }

        [Test]
        public void Dispose_LoggersIncludesNull_IgnoreNull()
        {
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(null);
            sut.loggers.Add(logger2);

            sut.Dispose();

            Assert.That(logger2.Disposed, Is.True);
        }

        [Test]
        public void Dispose_NestingLogger_IgnoreNested()
        {
            var logger2 = ScriptableObject.CreateInstance<SpyLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(sut); // nesting
            sut.loggers.Add(logger2);

            sut.Dispose();

            Assert.That(logger2.Disposed, Is.True);
        }
    }
}
