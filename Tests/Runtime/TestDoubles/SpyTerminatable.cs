// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace DeNA.Anjin.TestDoubles
{
    public class SpyTerminatable : ITerminatable
    {
        public bool IsCalled { get; private set; } = false;
        public ExitCode CapturedExitCode { get; private set; }
        public string CapturedMessage { get; private set; }
        public string CapturedStackTrace { get; private set; }
        public bool CapturedReporting { get; private set; }

        public async UniTask TerminateAsync(ExitCode exitCode, string message = null, string stackTrace = null,
            bool reporting = true, CancellationToken token = default)
        {
            IsCalled = true;
            CapturedExitCode = exitCode;
            CapturedMessage = message;
            CapturedStackTrace = stackTrace;
            CapturedReporting = reporting;
        }
    }
}
