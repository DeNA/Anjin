// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [SuppressMessage("ApiDesign", "RS0030")]
    public class UGUIMonkeyAgentTest
    {
        [SetUp]
        public async Task SetUp()
        {
            await EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
        }

        [Test]
        public async Task Run_cancelTask_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.lifespanSec = 0; // Expect indefinite execution
            agent.delayMillis = 100;

            var agentName = agent.GetType().Name;
            var gameObject = new GameObject(agentName);
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            Object.DestroyImmediate(gameObject);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5));

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        [Test]
        public async Task Run_lifespanPassed_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_lifespanPassed_stopAgent);
            agent.lifespanSec = 1;
            agent.delayMillis = 100;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2),
                    cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            }
        }

        [Test]
        public void Check_SelectableProperties()
        {
            var button = new GameObject("button1", new[] { typeof(RectTransform), typeof(Button) });
            var canvas = new GameObject().AddComponent<Canvas>();
            button.transform.parent = canvas.transform;

            // Verify properties needed to process with `UIDetector.Item.From()`.
            var selectable = button.GetComponent<Selectable>();
            Assert.That(selectable.interactable, Is.True);
            Assert.That(selectable.transform.GetType(), Is.EqualTo(typeof(RectTransform)));
            Assert.That(selectable.GetComponentInParent<Canvas>(), Is.Not.Null);
        }
    }
}
