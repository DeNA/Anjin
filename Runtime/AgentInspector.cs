// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;

namespace DeNA.Anjin
{
    /// <summary>
    /// Inspector for running agent.
    /// </summary>
    /// <remarks>
    /// In the future, it will display the agent's status.
    /// Currently, it is a component used to identify GameObjects.
    /// </remarks>
    public class AgentInspector : MonoBehaviour
    {
        /// <summary>
        /// Returns the running Agent instance array.
        /// No caching.
        /// </summary>
        public static AgentInspector[] Instances => FindObjectsOfType<AgentInspector>();
    }
}
