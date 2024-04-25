// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

#pragma warning disable CS0618 // Type or member is obsolete

namespace DeNA.Anjin
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [SuppressMessage("ApiDesign", "RS0030")]
    public class AgentDispatcherTest
    {
        private IAgentDispatcher _dispatcher;

        [SetUp]
        public void SetUp()
        {
            foreach (var agent in Object.FindObjectsOfType<AbstractAgent>())
            {
                Object.Destroy(agent);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _dispatcher?.Dispose();
        }

        private static AutopilotSettings CreateAutopilotSettings()
        {
            var testSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            testSettings.sceneAgentMaps = new List<SceneAgentMap>();
            return testSettings;
        }

        private static SpyAliveCountAgent CreateSpyAliveCountAgent(string name = nameof(DoNothingAgent))
        {
            var agent = ScriptableObject.CreateInstance<SpyAliveCountAgent>();
            agent.name = name;
            return agent;
        }

        private void SetUpDispatcher(AutopilotSettings settings)
        {
            var logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            var randomFactory = new RandomFactory(0);

            _dispatcher = new AgentDispatcher(settings, logger, randomFactory);
        }

        private const string TestScenePath = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";
        private const string TestScenePath2 = "Packages/com.dena.anjin/Tests/TestScenes/Error.unity";

        private static async UniTask<Scene> LoadTestSceneAsync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Scene scene = default;
#if UNITY_EDITOR
            scene = EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(mode));
            while (!scene.isLoaded)
            {
                await Task.Yield();
            }
#endif
            return scene;
        }

        [Test]
        public async Task DispatchByScene_DispatchAgentBySceneAgentMaps()
        {
            const string AgentName = "Mapped Agent";
            var settings = CreateAutopilotSettings();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateSpyAliveCountAgent(AgentName)
            });
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        public async Task DispatchByScene_DispatchFallbackAgent()
        {
            const string AgentName = "Fallback Agent";
            var settings = CreateAutopilotSettings();
            settings.fallbackAgent = CreateSpyAliveCountAgent(AgentName);
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        public async Task DispatchByScene_NoSceneAgentMapsAndFallbackAgent_AgentIsNotDispatch()
        {
            var settings = CreateAutopilotSettings();
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            LogAssert.Expect(LogType.Warning, "Agent not found by scene: Buttons");
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(0));
        }

        [Test]
        public async Task DispatchByScene_DispatchObserverAgent()
        {
            const string AgentName = "Observer Agent";
            var settings = CreateAutopilotSettings();
            settings.observerAgent = CreateSpyAliveCountAgent(AgentName);
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        public async Task DispatchByScene_ReActivateScene_NotCreateDuplicateAgents()
        {
            const string AgentName = "Mapped Agent";
            var settings = CreateAutopilotSettings();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateSpyAliveCountAgent(AgentName)
            });
            SetUpDispatcher(settings);

            var scene = await LoadTestSceneAsync(TestScenePath);
            Assume.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));

            var additiveScene = await LoadTestSceneAsync(TestScenePath2, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(additiveScene);

            SceneManager.SetActiveScene(scene); // Re-activate

            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1)); // Not create duplicate agents
        }
    }
}
