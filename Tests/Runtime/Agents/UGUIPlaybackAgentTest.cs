// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
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
    [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })] // Infinity loops inside the Automated QA package.
    [PrebuildSetup(typeof(CopyAssetsToResources))]
    [PostBuildCleanup(typeof(CleanupResources))]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public class UGUIPlaybackAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        [SetUp]
        public void SetUp()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.outputRootPath =
                Path.Combine(Application.temporaryCachePath, TestContext.CurrentContext.Test.ClassName);
            AutopilotState.Instance.settings = settings;
        }

        [TearDown]
        public void TearDown()
        {
            AutopilotState.Instance.Reset();
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_CancelTask_StopAgent()
        {
            var jsonPath = CopyAssetsToResources.GetAssetPath("TapButton1x4.json");

            var agent = ScriptableObject.CreateInstance<UGUIPlaybackAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = TestContext.CurrentContext.Test.Name;
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
        public async Task Run_PlaybackFinished_StopAgent()
        {
            var jsonPath = CopyAssetsToResources.GetAssetPath("TapButton1x1.json");

            var agent = ScriptableObject.CreateInstance<UGUIPlaybackAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = TestContext.CurrentContext.Test.Name;
            ;
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
