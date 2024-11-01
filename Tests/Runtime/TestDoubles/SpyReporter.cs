// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;
using UnityEngine;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// A spy for <c cref="AbstractReporter" />
    /// </summary>
    // [CreateAssetMenu(fileName = "New SpyReporter", menuName = "Anjin/Spy Reporter", order = 55)]
    public class SpyReporter : AbstractReporter
    {
        public List<Dictionary<string, string>> Arguments { get; } = new List<Dictionary<string, string>>();

        public override async UniTask PostReportAsync(
            string message,
            string stackTrace,
            ExitCode exitCode,
            CancellationToken cancellationToken = default
        )
        {
            Debug.Log("Reporter called");
            Arguments.Add(new Dictionary<string, string>
            {
                { "message", message }, { "stackTrace", stackTrace }, { "exitCode", exitCode.ToString() }
            });
            await UniTask.NextFrame(cancellationToken);
        }
    }
}
