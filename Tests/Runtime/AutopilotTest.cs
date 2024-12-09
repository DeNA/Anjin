// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DeNA.Anjin
{
    [TestFixture]
    public class AutopilotTest
    {
        private const string TestScenePath = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";
        private const string TestScenePath2 = "Packages/com.dena.anjin/Tests/TestScenes/Error.unity";

        private static DoNothingAgent CreateAgent()
        {
            var agent = ScriptableObject.CreateInstance<DoNothingAgent>();
            agent.name = "TestAgent";
            return agent;
        }

        private static AutopilotSettings CreateAutopilotSettings(int lifespanSec)
        {
            var settings = (AutopilotSettings)ScriptableObject.CreateInstance(typeof(AutopilotSettings));
            settings.sceneAgentMaps = new List<SceneAgentMap>();
            settings.fallbackAgent = CreateAgent(); // infinity alive
            settings.lifespanSec = lifespanSec;
            return settings;
        }

        [Test]
        public async Task Lifespan_Expire_CallTerminateAsync()
        {
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var settings = CreateAutopilotSettings(1);
            settings.reporters.Add(spyReporter);
            settings.exitCode = ExitCodeWhenLifespanExpired.Normally;
            settings.customExitCode = "100"; // dummy
            settings.exitMessage = "Lifespan expired";

            await Launcher.LaunchAutopilotAsync(settings);
            await UniTask.NextFrame(); // wait reporter

            Assert.That(spyReporter.IsCalled, Is.True);
            Assert.That(spyReporter.Arguments["exitCode"], Is.EqualTo(ExitCode.Normally.ToString()));
            Assert.That(spyReporter.Arguments["message"], Is.EqualTo("Lifespan expired"));
        }

        [Test]
        public async Task Lifespan_Expire_CallTerminateAsyncWithExitCodeAndMessage()
        {
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var settings = CreateAutopilotSettings(1);
            settings.reporters.Add(spyReporter);
            settings.exitCode = ExitCodeWhenLifespanExpired.Custom;
            settings.customExitCode = "100";
            settings.exitMessage = "Lifespan expired";

            try
            {
                await Launcher.LaunchAutopilotAsync(settings);
                Assert.Fail("No AssertionException was thrown");
            }
            catch (AssertionException)
            {
                // expected behavior
            }

            await UniTask.NextFrame(); // wait reporter

            Assert.That(spyReporter.IsCalled, Is.True);
            Assert.That(spyReporter.Arguments["exitCode"], Is.EqualTo("100"));
            Assert.That(spyReporter.Arguments["message"], Is.EqualTo("Lifespan expired"));
        }

        [Test]
        public async Task TerminateAsync_DestroyedAutopilotAndAgentObjects()
        {
            var settings = CreateAutopilotSettings(2);
            Launcher.LaunchAutopilotAsync(settings).Forget();
            await UniTask.Delay(500); // wait for launch

            var autopilot = Autopilot.Instance;
            Assume.That(autopilot, Is.Not.Null, "Autopilot is running");

            var agents = AgentInspector.Instances;
            Assume.That(agents, Is.Not.Empty, "Agents are running");

            await autopilot.TerminateAsync(ExitCode.Normally, reporting: false);
            await UniTask.NextFrame(); // wait for destroy

            autopilot = Object.FindObjectOfType<Autopilot>(); // re-find after terminated
            Assert.That(autopilot, Is.Null, "Autopilot was destroyed");

            agents = AgentInspector.Instances; // re-find after terminated
            Assert.That(agents, Is.Empty, "Agents were destroyed");
        }

        [Test]
        public async Task TerminateAsync_DuplicateCalls()
        {
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var settings = CreateAutopilotSettings(2);
            settings.reporters.Add(spyReporter);
            Launcher.LaunchAutopilotAsync(settings).Forget();
            await UniTask.Delay(500); // wait for launch

            var autopilot = Autopilot.Instance;
            autopilot.TerminateAsync(ExitCode.Normally, "1st call", null, true).Forget();
            autopilot.TerminateAsync(ExitCode.Normally, "2nd call", null, true).Forget();
            await UniTask.DelayFrame(2); // wait for destroy

            autopilot = Object.FindObjectOfType<Autopilot>(); // re-find after terminated
            Assert.That(autopilot, Is.Null, "Autopilot was destroyed");

            Assert.That(spyReporter.CallCount, Is.EqualTo(1));
            Assert.That(spyReporter.Arguments["message"], Is.EqualTo("1st call"));
        }

        [Test]
        public async Task TerminateAsync_NoReporting_NotCallReporter()
        {
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var settings = CreateAutopilotSettings(2);
            settings.reporters.Add(spyReporter);
            Launcher.LaunchAutopilotAsync(settings).Forget();
            await UniTask.Delay(500); // wait for launch

            var autopilot = Autopilot.Instance;
            await autopilot.TerminateAsync(ExitCode.Normally, null, null, false);

            Assert.That(spyReporter.IsCalled, Is.False);
        }

        [Test]
        public async Task TerminateAsync_Reporting_CallReporter()
        {
            var spyReporter = ScriptableObject.CreateInstance<SpyReporter>();
            var settings = CreateAutopilotSettings(2);
            settings.reporters.Add(spyReporter);
            Launcher.LaunchAutopilotAsync(settings).Forget();
            await UniTask.Delay(500); // wait for launch

            var autopilot = Autopilot.Instance;
            await autopilot.TerminateAsync(ExitCode.Normally, "message", "stack trace", true);

            Assert.That(spyReporter.IsCalled, Is.True);
            Assert.That(spyReporter.Arguments["exitCode"], Is.EqualTo("Normally"));
            Assert.That(spyReporter.Arguments["message"], Is.EqualTo("message"));
            Assert.That(spyReporter.Arguments["stackTrace"], Is.EqualTo("stack trace"));
        }

        [Test]
        public async Task Start_LoggerIsNotSet_UsingDefaultLogger()
        {
            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.lifespanSec = 1;

            await Launcher.LaunchAutopilotAsync(autopilotSettings);
            await UniTask.NextFrame(); // wait flushing log for subsequence tests

            LogAssert.Expect(LogType.Log, "Launched Autopilot"); // using console logger
        }

        [Test]
        public async Task Start_LoggerSpecified_UsingSpecifiedLogger()
        {
            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.lifespanSec = 1;
            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            autopilotSettings.loggerAssets.Add(spyLogger);

            await Launcher.LaunchAutopilotAsync(autopilotSettings);

            Assert.That(spyLogger.Logs, Does.Contain((LogType.Log, "Launched Autopilot"))); // using spy logger
            LogAssert.NoUnexpectedReceived(); // not write to console
        }

        [Test]
        [BuildScene(TestScenePath)]
        [BuildScene(TestScenePath2)]
        public async Task Start_MappedSceneIsNotActiveButLoaded_DispatchAgent()
        {
            const string MappedScenePath = TestScenePath;
            const string ActiveScenePath = TestScenePath2;

            var spyMappedAgent = ScriptableObject.CreateInstance<SpyAgent>();
            var spyFallbackAgent = ScriptableObject.CreateInstance<SpyAgent>();

            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.sceneAgentMaps = new List<SceneAgentMap>
            {
                new SceneAgentMap { scenePath = MappedScenePath, agent = spyMappedAgent }
            };
            autopilotSettings.fallbackAgent = spyFallbackAgent;
            autopilotSettings.lifespanSec = 1;

            await SceneManagerHelper.LoadSceneAsync(ActiveScenePath);
            await SceneManagerHelper.LoadSceneAsync(MappedScenePath, LoadSceneMode.Additive);
            Assume.That(SceneManager.GetActiveScene().path, Is.EqualTo(ActiveScenePath), "Mapped scene is not active");

            await Launcher.LaunchAutopilotAsync(autopilotSettings);

            Assert.That(spyMappedAgent.CompleteCount, Is.EqualTo(1), "Mapped Agent dispatched");
            Assert.That(spyFallbackAgent.CompleteCount, Is.EqualTo(0), "Fallback Agent is not dispatched");
        }

        [Test]
        [BuildScene(TestScenePath)]
        [BuildScene(TestScenePath2)]
        public async Task Start_NoMappedSceneInLoadedScenes_DispatchFallbackAgent()
        {
            const string AdditiveScenePath = TestScenePath;
            const string ActiveScenePath = TestScenePath2;

            var spyFallbackAgent = ScriptableObject.CreateInstance<SpyAgent>();

            var autopilotSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            autopilotSettings.fallbackAgent = spyFallbackAgent;
            autopilotSettings.lifespanSec = 1;

            await SceneManagerHelper.LoadSceneAsync(ActiveScenePath);
            await SceneManagerHelper.LoadSceneAsync(AdditiveScenePath, LoadSceneMode.Additive);

            await Launcher.LaunchAutopilotAsync(autopilotSettings);

            Assert.That(spyFallbackAgent.CompleteCount, Is.EqualTo(1));
        }
    }
}
