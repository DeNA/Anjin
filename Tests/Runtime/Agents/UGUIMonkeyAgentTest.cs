// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    [SuppressMessage("ApiDesign", "RS0030")]
    public class UGUIMonkeyAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";
        private readonly string _defaultOutputDirectory = CommandLineArgs.GetScreenshotDirectory();

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_cancelTask_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.lifespanSec = 0; // Expect indefinite execution
            agent.delayMillis = 100;

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
        [LoadScene(TestScene)]
        public async Task Run_lifespanPassed_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_lifespanPassed_stopAgent);
            agent.lifespanSec = 1;
            agent.delayMillis = 100;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(2000); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [FocusGameView]
        [LoadScene(TestScene)]
        public async Task Run_DefaultScreenshotFilenamePrefix_UseAgentName()
        {
            const string AgentName = "MyMonkeyAgent";
            var filename = $"{AgentName}01_0001.png";
            var path = Path.Combine(_defaultOutputDirectory, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Assume.That(path, Does.Not.Exist);

            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = AgentName;
            agent.lifespanSec = 1;
            agent.delayMillis = 100;
            agent.screenshotEnabled = true;
            agent.defaultScreenshotFilenamePrefix = true; // Use default prefix

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                await agent.Run(token);
            }

            Assert.That(path, Does.Exist);
        }
    }
}
