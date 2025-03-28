// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;

namespace DeNA.Anjin.TestDoubles
{
    public class SpyTerminatable : ITerminatable
    {
        public bool IsCalled { get; private set; } = false;
        public ExitCode CapturedExitCode { get; private set; }
        public string CapturedMessage { get; private set; }
        public string CapturedStackTrace { get; private set; }
        public bool CapturedReporting { get; private set; }

        public async UniTaskVoid TerminateAsync(ExitCode exitCode, string message = null, string stackTrace = null,
            bool reporting = true)
        {
            IsCalled = true;
            CapturedExitCode = exitCode;
            CapturedMessage = message;
            CapturedStackTrace = stackTrace;
            CapturedReporting = reporting;
            await UniTask.CompletedTask;
        }
    }
}
