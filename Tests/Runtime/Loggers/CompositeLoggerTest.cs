// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class CompositeLoggerTest
    {
        [Test]
        public void CompositeTwoLoggers_OutputBothLoggers()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var logger1 = ScriptableObject.CreateInstance<ConsoleLogger>();
            var logger2 = ScriptableObject.CreateInstance<ConsoleLogger>();
            var sut = ScriptableObject.CreateInstance<CompositeLogger>();
            sut.loggers.Add(logger1);
            sut.loggers.Add(logger2);

            sut.LoggerImpl.Log(message);

            LogAssert.Expect(LogType.Log, message);
            LogAssert.Expect(LogType.Log, message); // log output twice
            LogAssert.NoUnexpectedReceived();
        }
    }
}
