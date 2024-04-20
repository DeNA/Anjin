// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// An Agent that executes multiple Agents in parallel.
    /// </summary>
    [CreateAssetMenu(fileName = "New ParallelCompositeAgent", menuName = "Anjin/Parallel Composite Agent", order = 10)]
    public class ParallelCompositeAgent : AbstractCompositeAgent
    {
        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            var tasks = new UniTask[agents.Count];

            foreach (var (agent, i) in agents.Select((agent, i) => (agent, i)))
            {
                agent.Logger = Logger;
                agent.Random = RandomFactory.CreateRandom();
                tasks[i] = agent.Run(token);
            }

            try
            {
                await UniTask.WhenAll(tasks);
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
