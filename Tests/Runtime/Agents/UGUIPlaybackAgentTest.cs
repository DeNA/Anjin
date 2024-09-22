// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.TestUtils.Build;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin.Agents
{
    [TestFixture]
    [PrebuildSetup(typeof(CopyAssetsToResources))]
    [PostBuildCleanup(typeof(CleanupResources))]
    public class UGUIPlaybackAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_cancelTask_stopAgent()
        {
            var jsonPath = CopyAssetsToResources.GetAssetPath("TapButton1x4.json");

            var agent = ScriptableObject.CreateInstance<UGUIPlaybackAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
#if UNITY_EDITOR
            agent.recordedJson = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
#else
            agent.recordedJson = Resources.Load<TextAsset>(jsonPath);
#endif

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
        public async Task Run_playbackFinished_stopAgent()
        {
            var jsonPath = CopyAssetsToResources.GetAssetPath("TapButton1x1.json");

            var agent = ScriptableObject.CreateInstance<UGUIPlaybackAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_playbackFinished_stopAgent);
#if UNITY_EDITOR
            agent.recordedJson = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
#else
            agent.recordedJson = Resources.Load<TextAsset>(jsonPath);
#endif

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
        [LoadScene(TestScene)]
        public async Task Run_WithAssetFile()
        {
            var assetPath = CopyAssetsToResources.GetAssetPath("PlaybackAgent.asset");
#if UNITY_EDITOR
            var agent = AssetDatabase.LoadAssetAtPath<UGUIPlaybackAgent>(assetPath);
#else
            var agent = Resources.Load<UGUIPlaybackAgent>(assetPath);
#endif
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_WithAssetFile);

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
    }
}
