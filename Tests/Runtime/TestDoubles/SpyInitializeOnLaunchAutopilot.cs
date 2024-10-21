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
        public static bool IsCallInitializeOnLaunchAutopilotMethodNonPublic { get; private set; }
        public static bool IsCallInitializeOnLaunchAutopilotMethodTaskAsync { get; private set; }
        public static bool IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync { get; private set; }

        public static void Reset()
        {
            IsCallInitializeOnLaunchAutopilotMethod = false;
            IsCallInitializeOnLaunchAutopilotMethodNonPublic = false;
            IsCallInitializeOnLaunchAutopilotMethodTaskAsync = false;
            IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync = false;
        }

        [InitializeOnLaunchAutopilot]
        public static void InitializeOnLaunchAutopilotMethod()
        {
            IsCallInitializeOnLaunchAutopilotMethod = true;
        }

        [InitializeOnLaunchAutopilot]
        private static void InitializeOnLaunchAutopilotMethodNonPublic()
        {
            IsCallInitializeOnLaunchAutopilotMethodNonPublic = true;
        }

        [InitializeOnLaunchAutopilot]
        public static async Task InitializeOnLaunchAutopilotMethodTaskAsync()
        {
            await Task.Delay(0);
            IsCallInitializeOnLaunchAutopilotMethodTaskAsync = true;
        }

        [InitializeOnLaunchAutopilot]
        public static async UniTask InitializeOnLaunchAutopilotMethodUniTaskAsync()
        {
            await UniTask.Delay(0);
            IsCallInitializeOnLaunchAutopilotMethodUniTaskAsync = true;
        }
    }
}
