// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;

namespace DeNA.Anjin.Reporters.Slack.Payloads.Text
{
    /// <summary>
    /// Context Block.
    /// </summary>
    /// <remarks>
    /// Abstract classes and interfaces are not available. <c>JsonUtility.ToJson</c> can not process them.
    /// </remarks>
    /// <see href="https://api.slack.com/reference/block-kit/blocks"/>
    [Serializable]
    public class ContextBlock
    {
        public string type = "context";
        public Text[] elements; // Note: Text or Image

        public static ContextBlock CreateContextBlock(Text[] elements)
        {
            return new ContextBlock { elements = elements };
        }
    }
}
