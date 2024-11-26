// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class CompositeLoggerAssetTest
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
            var logger1 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var logger2 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var sut = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            sut.loggerAssets.Add(logger1);
            sut.loggerAssets.Add(logger2);

            var message = TestContext.CurrentContext.Test.Name;
            sut.Logger.Log(message);

            Assert.That(logger1.Logs, Does.Contain((LogType.Log, message)));
            Assert.That(logger2.Logs, Does.Contain((LogType.Log, message)));
        }

        [Test]
        public void LogFormat_LoggersIncludesNull_IgnoreNull()
        {
            var logger2 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var sut = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            sut.loggerAssets.Add(null);
            sut.loggerAssets.Add(logger2);

            var message = TestContext.CurrentContext.Test.Name;
            sut.Logger.Log(message);

            Assert.That(logger2.Logs, Does.Contain((LogType.Log, message)));
        }

        [Test]
        public void LogFormat_NestingLogger_IgnoreNested()
        {
            var logger2 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var sut = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            sut.loggerAssets.Add(sut); // nesting
            sut.loggerAssets.Add(logger2);

            var message = TestContext.CurrentContext.Test.Name;
            sut.Logger.Log(message);

            Assert.That(logger2.Logs, Does.Contain((LogType.Log, message)));
        }

        [Test]
        public void LogException_CompositeMultipleLoggers_OutputBothLoggers()
        {
            var logger1 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var logger2 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var sut = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            sut.loggerAssets.Add(logger1);
            sut.loggerAssets.Add(logger2);

            var message = TestContext.CurrentContext.Test.Name;
            var exception = CreateExceptionWithStacktrace(message);
            sut.Logger.LogException(exception);

            Assert.That(logger1.Logs, Does.Contain((LogType.Exception, exception.ToString())));
            Assert.That(logger2.Logs, Does.Contain((LogType.Exception, exception.ToString())));
        }

        [Test]
        public void LogException_LoggersIncludesNull_IgnoreNull()
        {
            var logger2 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var sut = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            sut.loggerAssets.Add(null);
            sut.loggerAssets.Add(logger2);

            var message = TestContext.CurrentContext.Test.Name;
            var exception = CreateExceptionWithStacktrace(message);
            sut.Logger.LogException(exception);

            Assert.That(logger2.Logs, Does.Contain((LogType.Exception, exception.ToString())));
        }

        [Test]
        public void LogException_NestingLogger_IgnoreNested()
        {
            var logger2 = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            var sut = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            sut.loggerAssets.Add(sut); // nesting
            sut.loggerAssets.Add(logger2);

            var message = TestContext.CurrentContext.Test.Name;
            var exception = CreateExceptionWithStacktrace(message);
            sut.Logger.LogException(exception);

            Assert.That(logger2.Logs, Does.Contain((LogType.Exception, exception.ToString())));
        }
    }
}
