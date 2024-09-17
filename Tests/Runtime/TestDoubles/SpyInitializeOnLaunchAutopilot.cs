// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Attributes;

namespace DeNA.Anjin.TestDoubles
{
    public static class SpyInitializeOnLaunchAutopilot
    {
        public static bool IsCallInitializeOnLaunchAutopilotMethod { get; private set; }

        public static void Reset()
        {
            IsCallInitializeOnLaunchAutopilotMethod = false;
        }

        [InitializeOnLaunchAutopilot]
        public static void InitializeOnLaunchAutopilotMethod()
        {
            IsCallInitializeOnLaunchAutopilotMethod = true;
        }
    }
}
