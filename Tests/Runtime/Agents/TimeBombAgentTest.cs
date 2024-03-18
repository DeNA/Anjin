// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Agents
{
    [TestFixture]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    public class TimeBombAgentTest
    {
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
            return agent;
        }

        [Test]
        public async Task Run_CancelTask_StopAgent()
        {
            var monkeyAgent = CreateMonkeyAgent(5);
            var agent = CreateTimeBombAgent(monkeyAgent, "^Never match!$");
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();

            using (var cts = new CancellationTokenSource())
            {
                var task = agent.Run(cts.Token);
                await UniTask.NextFrame();

                cts.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            }
        }

        [Test]
        [LoadScene("Packages/com.dena.anjin/Tests/TestScenes/OutGameTutorial.unity")]
        public async Task Run_Defuse_StopAgent()
        {
            var monkeyAgent = CreateMonkeyAgent(5);
            var agent = CreateTimeBombAgent(monkeyAgent, "^Tutorial Completed!$");
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();

            LogAssert.Expect(LogType.Log, "Working agent UGUIMonkeyAgent was cancelled.");
            LogAssert.Expect(LogType.Log, "Exit TimeBombAgent.Run()");

            using (var cts = new CancellationTokenSource())
            {
                await agent.Run(cts.Token);
            }
        }

        [Test]
        [LoadScene("Packages/com.dena.anjin/Tests/TestScenes/OutGameTutorial.unity")]
        public async Task Run_NotDefuse_ThrowsTimeoutException()
        {
            var monkeyAgent = CreateMonkeyAgent(1);
            var agent = CreateTimeBombAgent(monkeyAgent, "^Never match!$");
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();

            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    await agent.Run(cts.Token);
                    Assert.Fail("Should throw TimeoutException");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo(
                        "Could not receive defuse message `^Never match!$` before the agent terminated."));
                }
            }
        }
    }
}
