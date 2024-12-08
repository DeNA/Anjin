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
        // @formatter:off
        private static readonly string s_requireReset = L10n.Tr("Autopilot has an invalid running state. Please click the \"Reset\" button.");
        private static readonly string s_resetButton = L10n.Tr("Reset");
        // @formatter:on

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            var state = AutopilotState.Instance;

            if (state.IsInvalidState)
            {
                EditorGUILayout.HelpBox(s_requireReset, MessageType.Error);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!state.IsInvalidState);
            }

            if (GUILayout.Button(s_resetButton))
            {
                state.Reset();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}
