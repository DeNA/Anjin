// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Settings;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Settings
{
    /// <summary>
    /// Autopilot State inspector UI
    /// </summary>
    [CustomEditor(typeof(AutopilotState))]
    public class AutopilotStateEditor : UnityEditor.Editor
    {
        private static readonly string s_resetButton = L10n.Tr("Reset");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            var state = AutopilotState.Instance;

            EditorGUI.BeginDisabledGroup(!state.IsRunning || EditorApplication.isPlayingOrWillChangePlaymode);
            if (GUILayout.Button(s_resetButton))
            {
                state.Reset();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}
