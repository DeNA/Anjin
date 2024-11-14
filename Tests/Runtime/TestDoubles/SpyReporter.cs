// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// A spy for <c cref="AbstractReporter" />
    /// </summary>
    public class SpyReporter : AbstractReporter
    {
        public int CallCount { get; private set; }
        public bool IsCalled => CallCount > 0;
        public Dictionary<string, string> Arguments { get; } = new Dictionary<string, string>();

        public override async UniTask PostReportAsync(
            string message,
            string stackTrace,
            ExitCode exitCode,
            CancellationToken cancellationToken = default
        )
        {
            this.CallCount++;
            Arguments.Add("message", message);
            Arguments.Add("stackTrace", stackTrace);
            Arguments.Add("exitCode", exitCode.ToString());
            await UniTask.NextFrame(cancellationToken);
        }
    }
}
