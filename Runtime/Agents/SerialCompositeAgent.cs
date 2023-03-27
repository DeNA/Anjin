// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// An Agent that executes multiple Agents in series.
    /// </summary>
    [CreateAssetMenu(fileName = "New SerialCompositeAgent", menuName = "Anjin/Serial Composite Agent", order = 11)]
    public class SerialCompositeAgent : AbstractCompositeAgent
    {
        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            foreach (var agent in agents)
            {
                agent.Logger = Logger;
                agent.Random = RandomFactory.CreateRandom();
                await agent.Run(token);
            }

            Logger.Log($"Exit {this.name}.Run()");
        }
    }
}
