// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.TestComponents;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Agents
{
    [TestFixture]
    public class TimeBombAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/OutGameTutorial.unity";

        [SetUp]
        public void SetUp()
        {
            OutGameTutorialButton.ResetTutorialCompleted();
        }

        private static UGUIMonkeyAgent CreateMonkeyAgent(long lifespanSec)
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.name = nameof(UGUIMonkeyAgent);
            agent.lifespanSec = lifespanSec;
            agent.delayMillis = 100;
            agent.secondsToErrorForNoInteractiveComponent = 0; // Disable check
            agent.touchAndHoldDelayMillis = 200;
            return agent;
        }

        private static TimeBombAgent CreateTimeBombAgent(AbstractAgent workingAgent, string defuseMessage)
        {
            var agent = ScriptableObject.CreateInstance<TimeBombAgent>();
            agent.name = nameof(TimeBombAgent);
            agent.agent = workingAgent;
            agent.defuseMessage = defuseMessage;
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            return agent;
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_CancelTask_StopAgent()
        {
            var monkeyAgent = CreateMonkeyAgent(5);
            var agent = CreateTimeBombAgent(monkeyAgent, "^Never match!$");

            using (var cts = new CancellationTokenSource())
            {
                var task = agent.Run(cts.Token);
                await UniTask.NextFrame();

                cts.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Enter {monkeyAgent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {monkeyAgent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_Defuse_StopAgent()
        {
            var monkeyAgent = CreateMonkeyAgent(5);
            var agent = CreateTimeBombAgent(monkeyAgent, "^Tutorial Completed!$");

            using (var cts = new CancellationTokenSource())
            {
                await agent.Run(cts.Token);
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Enter {monkeyAgent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {monkeyAgent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Working agent {monkeyAgent.name} was cancelled.");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_NotDefuse_AutopilotFailed()
        {
            var monkeyAgent = CreateMonkeyAgent(1);
            var agent = CreateTimeBombAgent(monkeyAgent, "^Never match!$");
            var spyTerminatable = new SpyTerminatable();
            agent._autopilot = spyTerminatable;

            using (var cts = new CancellationTokenSource())
            {
                await agent.Run(cts.Token);
                await UniTask.NextFrame();
            }

            Assert.That(spyTerminatable.IsCalled, Is.True);
            Assert.That(spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
            Assert.That(spyTerminatable.CapturedMessage, Is.EqualTo(
                "Could not receive defuse message `^Never match!$` before the UGUIMonkeyAgent terminated."));
            Assert.That(spyTerminatable.CapturedStackTrace, Does.StartWith(
                "  at DeNA.Anjin.Agents.TimeBombAgent.Run"));
            Assert.That(spyTerminatable.CapturedReporting, Is.True);

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Enter {monkeyAgent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {monkeyAgent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }
    }
}
