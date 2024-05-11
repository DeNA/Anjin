// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class ConsoleLoggerAssetTest
    {
        [Test]
        public void FilterLogTypeIsNotSet_LogAsDefault()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var sut = ScriptableObject.CreateInstance<ConsoleLoggerAsset>();

            sut.Logger.Log(message);

            LogAssert.Expect(LogType.Log, message);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void FilterLogTypeSpecified(
            [Values(LogType.Error, LogType.Assert, LogType.Warning, LogType.Exception)]
            LogType logType)
        {
            var message = TestContext.CurrentContext.Test.Name;
            var sut = ScriptableObject.CreateInstance<ConsoleLoggerAsset>();
            sut.filterLogType = logType;

            sut.Logger.Log(logType, message);
            sut.Logger.Log("Not output message because LogType.Log");

            LogAssert.Expect(logType, message);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
