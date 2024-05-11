// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin
{
    [TestFixture]
    public class AutopilotTest
    {
        [Test]
        public async Task Start_LoggerIsNotSet_UsingDefaultLogger()
        {
            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.sceneAgentMaps = new List<SceneAgentMap>();
            autopilotSettings.lifespanSec = 1;

            await LauncherFromTest.AutopilotAsync(autopilotSettings);

            LogAssert.Expect(LogType.Log, "Launched autopilot"); // using console logger
        }

        [Test]
        public async Task Start_LoggerSpecified_UsingSpecifiedLogger()
        {
            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.sceneAgentMaps = new List<SceneAgentMap>();
            autopilotSettings.lifespanSec = 1;
            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            autopilotSettings.loggerAsset = spyLogger;

            await LauncherFromTest.AutopilotAsync(autopilotSettings);

            Assert.That(spyLogger.Logs, Does.Contain("Launched autopilot")); // using spy logger
            LogAssert.NoUnexpectedReceived(); // not write to console
        }
    }
}
