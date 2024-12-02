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
#if UNITY_2022_3_OR_NEWER
        public static AgentInspector[] Instances => FindObjectsByType<AgentInspector>(FindObjectsSortMode.None);
        // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
        public static AgentInspector[] Instances => FindObjectsOfType<AgentInspector>();
#endif
    }
}
