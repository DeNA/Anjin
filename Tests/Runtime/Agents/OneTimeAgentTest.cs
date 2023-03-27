// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    public class OneTimeAgentTest
    {
        private static int s_childAgentCount;

        private static SpyAgent CreateChildAgent(long lifespanMillis)
        {
            var agent = ScriptableObject.CreateInstance<SpyAgent>();
            agent.name = $"ChildAgent {++s_childAgentCount}";
            agent.lifespanMillis = lifespanMillis;
            return agent;
        }

        [Test]
        public async Task Run_cancelTask_stopAgent()
        {
            var childAgent = CreateChildAgent(5000);

            var agent = ScriptableObject.CreateInstance<OneTimeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;

            var agentName = agent.GetType().Name;
            var gameObject = new GameObject(agentName);
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(TimeSpan.FromMilliseconds(200), cancellationToken: token);

            Object.DestroyImmediate(gameObject);
            // ReSharper disable once MethodSupportsCancellation
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        [Test]
        public async Task Run_markWasExecuted()
        {
            var childAgent = CreateChildAgent(100);

            var agent = ScriptableObject.CreateInstance<OneTimeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
                Assert.That(agent.wasExecuted, Is.True);
            }
        }

        [Test]
        public async Task Run_wasExecuted_notExecuteChildAgent()
        {
            var childAgent = CreateChildAgent(100);

            var agent = ScriptableObject.CreateInstance<OneTimeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;
            agent.wasExecuted = true;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
                Assert.That(childAgent.CompleteCount, Is.EqualTo(0)); // Skip run child agent
            }
        }

        [Test]
        public async Task Run_setLoggerAndRandomInstanceToChildAgent()
        {
            var childAgent = CreateChildAgent(100);

            var agent = ScriptableObject.CreateInstance<OneTimeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
                Assert.That(childAgent.Logger, Is.Not.Null);
                Assert.That(childAgent.Logger, Is.EqualTo(agent.Logger)); // Instances inherited from parent
                Assert.That(childAgent.Random, Is.Not.Null);
                Assert.That(childAgent.Random, Is.EqualTo(agent.Random)); // Instances inherited from parent
            }
        }
    }
}
