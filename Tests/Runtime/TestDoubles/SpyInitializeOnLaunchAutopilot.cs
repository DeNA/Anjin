// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;

namespace DeNA.Anjin.TestDoubles
{
    public static class SpyInitializeOnLaunchAutopilot
    {
        public static bool WasCalled { get; private set; }
        public static bool WasCalledNonPublicMethod { get; private set; }
        public static bool WasCalledTaskAsync { get; private set; }
        public static bool WasCalledUniTaskAsync { get; private set; }

        public static void Reset()
        {
            WasCalled = false;
            WasCalledNonPublicMethod = false;
            WasCalledTaskAsync = false;
            WasCalledUniTaskAsync = false;
        }

        [InitializeOnLaunchAutopilot]
        public static void InitializeOnLaunchAutopilotMethod()
        {
            WasCalled = true;
        }

        [InitializeOnLaunchAutopilot]
        private static void InitializeOnLaunchAutopilotNonPublicMethod()
        {
            WasCalledNonPublicMethod = true;
        }

        [InitializeOnLaunchAutopilot]
        public static async Task InitializeOnLaunchAutopilotTaskAsync()
        {
            await Task.Delay(0);
            WasCalledTaskAsync = true;
        }

        [InitializeOnLaunchAutopilot]
        public static async UniTask InitializeOnLaunchAutopilotUniTaskAsync()
        {
            await UniTask.Delay(0);
            WasCalledUniTaskAsync = true;
        }
    }
}
