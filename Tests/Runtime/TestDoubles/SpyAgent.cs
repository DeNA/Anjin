// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// Spy agent that count the number of called times
    /// </summary>
    // ReSharper disable once RequiredBaseTypesIsNotInherited
    public class SpyAgent : AbstractAgent
    {
        public long lifespanMillis;
        public int CompleteCount { get; private set; }

        public override async UniTask Run(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(lifespanMillis), cancellationToken: token);
            CompleteCount++;
        }
    }
}
