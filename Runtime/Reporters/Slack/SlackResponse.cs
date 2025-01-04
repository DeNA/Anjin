// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin.Reporters.Slack
{
    /// <summary>
    /// Slack post message API response
    /// </summary>
    public struct SlackResponse
    {
        /// <summary>
        /// Success/failure
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Thread timestamp
        /// </summary>
        public string Ts { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">Post was success</param>
        /// <param name="ts">Thread timestamp</param>
        public SlackResponse(bool success, string ts = null)
        {
            Success = success;
            Ts = ts;
        }
    }
}
