// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DeNA.Anjin
{
    /// <summary>
    /// Agent dispatcher interface
    /// </summary>
    public interface IAgentDispatcher : IDisposable
    {
        /// <summary>
        /// Agent dispatch by current scene
        /// </summary>
        /// <param name="scene">Current scene</param>
        /// <param name="fallback">Use fallback agent if true</param>
        /// <returns>True: Agent dispatched</returns>
        bool DispatchByScene(Scene scene, bool fallback = true);

        /// <summary>
        /// Dispatch scene crossing Agents. 
        /// </summary>
        void DispatchSceneCrossingAgents();
    }

    /// <inheritdoc/>
    public class AgentDispatcher : IAgentDispatcher
    {
        private readonly AutopilotSettings _settings;
        private readonly ILogger _logger;
        private readonly RandomFactory _randomFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        /// <param name="randomFactory"></param>
        public AgentDispatcher(AutopilotSettings settings, ILogger logger, RandomFactory randomFactory)
        {
            _settings = settings;
            _logger = logger;
            _randomFactory = randomFactory;

            SceneManager.sceneLoaded += this.DispatchByScene;
        }

        public void Dispose()
        {
            foreach (var inspector in AgentInspector.Instances)
            {
                _logger.Log($"Destroy running Agent: {inspector.gameObject.name}");
                Object.Destroy(inspector.gameObject);
            }

            SceneManager.sceneLoaded -= this.DispatchByScene;
        }

        private void DispatchByScene(Scene next, LoadSceneMode mode)
        {
            DispatchByScene(next);
        }

        /// <inheritdoc/>
        public bool DispatchByScene(Scene scene, bool fallback = true)
        {
            AbstractAgent agent = null;

            foreach (var sceneAgentMap in _settings.sceneAgentMaps)
            {
                if (sceneAgentMap.scenePath.Equals(scene.path))
                {
                    agent = sceneAgentMap.agent;
                    break;
                }
            }

            if (!agent)
            {
                if (_settings.fallbackAgent && fallback)
                {
                    _logger.Log($"Use fallback Agent. Scene: {scene.name}");
                    agent = _settings.fallbackAgent;
                }
                else
                {
                    _logger.Log(LogType.Warning, $"Agent not specified for Scene: {scene.name}");
                }
            }

            if (!agent)
            {
                return false;
            }

            DispatchAgent(agent);
            return true;
        }

        /// <inheritdoc/>
        public void DispatchSceneCrossingAgents()
        {
            _settings.sceneCrossingAgents.ForEach(agent =>
            {
                DispatchAgent(agent, true);
            });
        }

        private void DispatchAgent(AbstractAgent agent, bool dontDestroyOnLoad = false)
        {
            var agentName = agent.name;
            agent.Logger = _logger;
            agent.Random = _randomFactory.CreateRandom();

            var inspector = new GameObject(agentName).AddComponent<AgentInspector>();
            if (dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(inspector.gameObject);
            }

            var token = inspector.gameObject.GetCancellationTokenOnDestroy();
            _logger.Log($"Dispatch Agent: {agentName}");
            agent.Run(token).Forget(); // Agent also dies when GameObject is destroyed
        }
    }
}
