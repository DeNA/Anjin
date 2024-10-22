// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DeNA.Anjin
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [SuppressMessage("ApiDesign", "RS0030")]
    public class AgentDispatcherTest
    {
        private IAgentDispatcher _dispatcher;

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

        private static SpyAliveCountAgent CreateSpyAliveCountAgent(string name = nameof(SpyAliveCountAgent))
        {
            var agent = ScriptableObject.CreateInstance<SpyAliveCountAgent>();
            agent.name = name;
            return agent;
        }

        private void SetUpDispatcher(AutopilotSettings settings)
        {
            var logger = Debug.unityLogger;
            var randomFactory = new RandomFactory(0);

            _dispatcher = new AgentDispatcher(settings, logger, randomFactory);
        }

        private const string TestScenePath = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";
        private const string TestScenePath2 = "Packages/com.dena.anjin/Tests/TestScenes/Error.unity";

        private static string GetSceneName(string path)
        {
            var pattern = new Regex(@"([^/]+)\.unity$");
            return pattern.Match(path).Groups[1].Value;
        }

        [Test]
        [CreateScene]
        public async Task DispatchByScene_DispatchAgentBySceneAgentMaps()
        {
            const string AgentName = "Mapped Agent";
            var settings = CreateAutopilotSettings();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateSpyAliveCountAgent(AgentName)
            });
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        [CreateScene]
        public async Task DispatchByScene_DispatchFallbackAgent()
        {
            const string AgentName = "Fallback Agent";
            var settings = CreateAutopilotSettings();
            settings.fallbackAgent = CreateSpyAliveCountAgent(AgentName);
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        [CreateScene]
        public async Task DispatchByScene_NoSceneAgentMapsAndFallbackAgent_AgentIsNotDispatch()
        {
            var settings = CreateAutopilotSettings();
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);

            LogAssert.Expect(LogType.Warning, "Agent not found by scene: Buttons");
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(0));
        }

        [Test]
        [CreateScene]
        public async Task DispatchByScene_DispatchObserverAgent()
        {
            const string AgentName = "Observer Agent";
            var settings = CreateAutopilotSettings();
            settings.observerAgent = CreateSpyAliveCountAgent(AgentName);
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        [CreateScene]
        public async Task DispatchByScene_ReActivateScene_NotCreateDuplicateAgents()
        {
            const string AgentName = "Mapped Agent";
            var settings = CreateAutopilotSettings();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateSpyAliveCountAgent(AgentName)
            });
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);
            var scene = SceneManager.GetSceneByName(GetSceneName(TestScenePath));
            Assume.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));

            await SceneManagerHelper.LoadSceneAsync(TestScenePath2, LoadSceneMode.Additive);
            var additiveScene = SceneManager.GetSceneByName(GetSceneName((TestScenePath2)));
            SceneManager.SetActiveScene(additiveScene);

            SceneManager.SetActiveScene(scene); // Re-activate

            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1)); // Not create duplicate agents
        }
    }
}
