// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// An Agent that can register one child Agent and execute it only once throughout the Autopilot execution period.
    /// The second time executions will be skipped.
    /// </summary>
    [CreateAssetMenu(fileName = "New OneTimeAgent", menuName = "Anjin/One Time Agent", order = 16)]
    public class OneTimeAgent : AbstractAgent
    {
        /// <summary>
        /// Agent will only run once for the entire autopilot run period
        /// </summary>
        public AbstractAgent agent;

        [SerializeField] [HideInInspector] internal bool wasExecuted;

        [InitializeOnLaunchAutopilot]
        public static void ResetExecutedFlag()
        {
            // Reset runtime instances
            var oneTimeAgents = FindObjectsOfType<OneTimeAgent>();
            foreach (var current in oneTimeAgents)
            {
                current.wasExecuted = false;
            }
#if UNITY_EDITOR
            // Reset asset files (in Editor only)
            foreach (var guid in AssetDatabase.FindAssets("t:OneTimeAgent"))
            {
                var so = AssetDatabase.LoadAssetAtPath<OneTimeAgent>(AssetDatabase.GUIDToAssetPath(guid));
                so.wasExecuted = false;
            }
#endif
        }

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            if (wasExecuted)
            {
                Logger.Log($"Skip {this.name} since it has already been executed");
                return;
            }

            wasExecuted = true;

            Logger.Log($"Enter {this.name}.Run()");

            agent.Logger = Logger;
            agent.Random = Random; // This Agent does not consume pseudo-random numbers, so passed on as is.
            try
            {
                await agent.Run(token);
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
