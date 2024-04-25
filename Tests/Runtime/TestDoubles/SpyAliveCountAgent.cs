// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using UnityEngine;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// Spy agent that count the number of alive instances.
    /// </summary>
    [AddComponentMenu("")]
    // ReSharper disable once RequiredBaseTypesIsNotInherited
    public class SpyAliveCountAgent : AbstractAgent
    {
        public static int AliveInstances { get; private set; }

        public override async UniTask Run(CancellationToken token)
        {
            AliveInstances++;
            try
            {
                await UniTask.WaitWhile(() => true, cancellationToken: token); // Wait indefinitely
            }
            finally
            {
                AliveInstances--;
            }
        }
    }
}
