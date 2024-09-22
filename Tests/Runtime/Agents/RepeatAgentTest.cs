// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

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
    public class RepeatAgentTest
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
            var childAgent = CreateChildAgent(100);

            var agent = ScriptableObject.CreateInstance<RepeatAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;

            var gameObject = new GameObject();
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.NextFrame();

            Object.DestroyImmediate(gameObject);
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        public async Task Run_childAgentRunRepeating()
        {
            var childAgent = CreateChildAgent(100);

            var agent = ScriptableObject.CreateInstance<RepeatAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;

            var gameObject = new GameObject();
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(500);

            Object.DestroyImmediate(gameObject);
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(childAgent.CompleteCount, Is.GreaterThan(2));

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        public async Task Run_setLoggerAndRandomInstanceToChildAgent()
        {
            var childAgent = CreateChildAgent(100);

            var agent = ScriptableObject.CreateInstance<RepeatAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.agent = childAgent;

            var gameObject = new GameObject();
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(500);

            Object.DestroyImmediate(gameObject);
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(childAgent.Logger, Is.EqualTo(agent.Logger)); // Instances inherited from parent
            Assert.That(childAgent.Random, Is.EqualTo(agent.Random)); // Instances inherited from parent

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }
    }
}
