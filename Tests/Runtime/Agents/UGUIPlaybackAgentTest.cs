// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    public class UGUIPlaybackAgentTest
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
            var agent = ScriptableObject.CreateInstance<UGUIPlaybackAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.recordedJson = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Packages/com.dena.anjin/Tests/TestAssets/TapButton1x4.json");

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
        public async Task Run_playbackFinished_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIPlaybackAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_playbackFinished_stopAgent);
            agent.recordedJson = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Packages/com.dena.anjin/Tests/TestAssets/TapButton1x1.json");

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            }
        }
    }
}
