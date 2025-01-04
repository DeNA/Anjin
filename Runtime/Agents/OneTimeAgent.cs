// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
using UnityEngine;

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

        public bool WasExecuted { get; internal set; }

        [InitializeOnLaunchAutopilot]
        public static void ResetExecutedFlag()
        {
            // Reset runtime instances
            var oneTimeAgents = Resources.FindObjectsOfTypeAll<OneTimeAgent>();
            foreach (var current in oneTimeAgents)
            {
                current.WasExecuted = false;
            }
        }

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken cancellationToken)
        {
            if (WasExecuted)
            {
                Logger.Log($"Skip {this.name} since it has already been executed");
                return;
            }

            WasExecuted = true;

            Logger.Log($"Enter {this.name}.Run()");

            agent.Logger = Logger;
            agent.Random = Random; // This Agent does not consume pseudo-random numbers, so passed on as is.
            try
            {
                await agent.Run(cancellationToken);
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
