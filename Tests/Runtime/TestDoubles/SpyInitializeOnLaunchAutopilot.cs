// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;

namespace DeNA.Anjin.TestDoubles
{
    public static class SpyInitializeOnLaunchAutopilot
    {
        public static bool IsCallInitializeOnLaunchAutopilotMethod { get; private set; }
        public static bool IsCallInitializeOnLaunchAutopilotMethodAsync { get; private set; }
        public static bool IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync { get; private set; }

        public static void Reset()
        {
            IsCallInitializeOnLaunchAutopilotMethod = false;
            IsCallInitializeOnLaunchAutopilotMethodAsync = false;
            IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync = false;
        }

        [InitializeOnLaunchAutopilot]
        public static void InitializeOnLaunchAutopilotMethod()
        {
            IsCallInitializeOnLaunchAutopilotMethod = true;
        }

        [InitializeOnLaunchAutopilot]
        public static async Task InitializeOnLaunchAutopilotMethodAsync()
        {
            await Task.Delay(0);
            IsCallInitializeOnLaunchAutopilotMethodAsync = true;
        }

        [InitializeOnLaunchAutopilot]
        public static async UniTask InitializeOnLaunchAutopilotMethodUniTaskAsync()
        {
            await UniTask.Delay(0);
            IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync = true;
        }
    }
}
