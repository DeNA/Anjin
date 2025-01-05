// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

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
    [TestFixture]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    public class AgentDispatcherTest
    {
        private const string TestScenePath = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";
        private const string TestScenePath2 = "Packages/com.dena.anjin/Tests/TestScenes/Error.unity";

        private IAgentDispatcher _dispatcher;

        private static string GetSceneName(string path)
        {
            var pattern = new Regex(@"([^/]+)\.unity$");
            return pattern.Match(path).Groups[1].Value;
        }

        [TearDown]
        public void TearDown()
        {
            _dispatcher?.Dispose();
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

        [Test]
        [CreateScene]
        public async Task DispatchByScene_DispatchAgentBySceneAgentMaps()
        {
            const string AgentName = "Mapped Agent";
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
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
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
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
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);

            LogAssert.Expect(LogType.Warning, "Agent not specified for Scene: Buttons");
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(0));
        }

        [Test]
        [CreateScene]
        public async Task DispatchByScene_ReActivateScene_NotCreateDuplicateAgents()
        {
            const string AgentName = "Mapped Agent";
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateSpyAliveCountAgent(AgentName)
            });
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath);
            var scene = SceneManager.GetSceneByName(GetSceneName(TestScenePath));
            Assume.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));

            await SceneManagerHelper.LoadSceneAsync(TestScenePath2, LoadSceneMode.Additive);
            var additiveScene = SceneManager.GetSceneByName(GetSceneName(TestScenePath2));
            SceneManager.SetActiveScene(additiveScene);

            SceneManager.SetActiveScene(scene); // Re-activate

            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1)); // Not create duplicate agents
        }

        [Test]
        [CreateScene(unloadOthers: true)]
        public async Task DispatchByScene_SceneUnloadWithoutBecomingActive_DisposeAgent()
        {
            const string AgentName = "Mapped Agent";
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateSpyAliveCountAgent(AgentName)
            });
            SetUpDispatcher(settings);

            await SceneManagerHelper.LoadSceneAsync(TestScenePath, LoadSceneMode.Additive);
            Assume.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));

            await SceneManager.UnloadSceneAsync(GetSceneName(TestScenePath));
            await UniTask.NextFrame(); // Wait for destroy

            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(0));
        }

        [Test]
        [CreateScene]
        public void DispatchSceneCrossingAgents_DispatchAgent()
        {
            const string AgentName = "Scene Crossing Agent";
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.sceneCrossingAgents.Add(CreateSpyAliveCountAgent(AgentName));
            SetUpDispatcher(settings);

            _dispatcher.DispatchSceneCrossingAgents();

            var gameObject = GameObject.Find(AgentName);
            Assert.That(gameObject, Is.Not.Null);
            Assert.That(SpyAliveCountAgent.AliveInstances, Is.EqualTo(1));
        }

        [Test]
        public async Task Dispose_DestroyAllRunningAgents()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.fallbackAgent = CreateSpyAliveCountAgent("Fallback Agent");
            settings.sceneCrossingAgents.Add(CreateSpyAliveCountAgent("Scene Crossing Agent"));
            SetUpDispatcher(settings);
            _dispatcher.DispatchSceneCrossingAgents();
            await SceneManagerHelper.LoadSceneAsync(TestScenePath);

            _dispatcher.Dispose();
            await UniTask.NextFrame(); // Wait for destroy

            var agents = Object.FindObjectsOfType<AgentInspector>();
            Assert.That(agents, Is.Empty, "Agents were destroyed");
        }
    }
}
