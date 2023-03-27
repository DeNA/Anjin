// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
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
    public class ParallelCompositeAgentTest
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
            var firstChildAgent = CreateChildAgent(5000);
            var secondChildAgent = CreateChildAgent(5000);
            var lastChildAgent = CreateChildAgent(5000);

            var agent = ScriptableObject.CreateInstance<ParallelCompositeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agents = new List<AbstractAgent>() { firstChildAgent, secondChildAgent, lastChildAgent };

            var agentName = agent.GetType().Name;
            var gameObject = new GameObject(agentName);
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            Object.DestroyImmediate(gameObject);
            // ReSharper disable once MethodSupportsCancellation
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        [Test]
        public async Task Run_lifespanPassed_stopAgent()
        {
            var firstChildAgent = CreateChildAgent(500);
            var secondChildAgent = CreateChildAgent(500);
            var lastChildAgent = CreateChildAgent(500);

            var agent = ScriptableObject.CreateInstance<ParallelCompositeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_lifespanPassed_stopAgent);
            agent.agents = new List<AbstractAgent>() { firstChildAgent, secondChildAgent, lastChildAgent };

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
                Assert.That(firstChildAgent.CompleteCount, Is.EqualTo(1));
                Assert.That(secondChildAgent.CompleteCount, Is.EqualTo(1));
                Assert.That(lastChildAgent.CompleteCount, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Run_setLoggerAndRandomInstanceToChildAgent()
        {
            var firstChildAgent = CreateChildAgent(500);
            var secondChildAgent = CreateChildAgent(500);
            var lastChildAgent = CreateChildAgent(500);

            var agent = ScriptableObject.CreateInstance<ParallelCompositeAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_setLoggerAndRandomInstanceToChildAgent);
            agent.agents = new List<AbstractAgent>() { firstChildAgent, secondChildAgent, lastChildAgent };

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
                Assert.That(firstChildAgent.Logger, Is.Not.Null);
                Assert.That(firstChildAgent.Logger, Is.EqualTo(agent.Logger)); // Instances inherited from parent
                Assert.That(firstChildAgent.Random, Is.Not.Null);
                Assert.That(firstChildAgent.Random, Is.Not.EqualTo(agent.Random));
                Assert.That(firstChildAgent.Random, Is.Not.EqualTo(secondChildAgent.Random));

                var firstRandomValue = firstChildAgent.Random.Next();
                Assert.That(firstRandomValue, Is.Not.EqualTo(agent.Random.Next()));
                Assert.That(firstRandomValue, Is.Not.EqualTo(secondChildAgent.Random.Next()));
            }
        }
    }
}
