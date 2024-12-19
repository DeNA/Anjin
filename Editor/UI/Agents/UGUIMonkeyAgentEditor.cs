// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for UGUIMonkeyAgent
    /// </summary>
    [CustomEditor(typeof(UGUIMonkeyAgent))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase")]
    public class UGUIMonkeyAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Agent instance.");
        private SerializedProperty _descriptionProp;
        private GUIContent _descriptionGUIContent;

        private static readonly string s_lifespanSec = L10n.Tr("Lifespan [sec]");

        private static readonly string s_lifespanSecTooltip =
            L10n.Tr("Agent running lifespan [sec]. When specified zero, so unlimited running.");

        private SerializedProperty _lifespanProp;
        private GUIContent _lifespanGUIContent;

        private static readonly string s_delayMillis = L10n.Tr("Delay [millis]");
        private static readonly string s_delayMillisTooltip = L10n.Tr("Delay time between random operations [ms]");
        private SerializedProperty _delayMillisProp;
        private GUIContent _delayMillisGUIContent;

        private static readonly string s_timeout =
            L10n.Tr("No-Element Timeout [sec]");

        private static readonly string s_timeoutToolTip = L10n.Tr(
            "Abort this Agent when the interactable UI/2D/3D element does not appear for the specified seconds."
        );

        private SerializedProperty _timeoutProp;
        private GUIContent _timeoutGUIContent;

        private static readonly string s_gizmos = L10n.Tr("Enable Gizmos");

        private static readonly string s_gizmosTooltip =
            L10n.Tr("Show Gizmos on GameView during running monkey test if true");

        private SerializedProperty _gizmosProp;
        private GUIContent _gizmosGUIContent;

        // Screenshot options
        private static readonly string s_screenshotOptions = L10n.Tr("Screenshot Options");
        private static readonly string s_screenshotEnabled = L10n.Tr("Enabled");
        private static readonly string s_screenshotEnabledTooltip = L10n.Tr("Whether screenshot is enabled or not");
        private SerializedProperty _screenshotEnabledProp;
        private GUIContent _screenshotEnabledGUIContent;

        private static readonly string s_screenshotFilename = L10n.Tr("Filename");
        private static readonly string s_screenshotDefaultFilenamePrefix = L10n.Tr("Use Default");

        private static readonly string s_screenshotDefaultFilenamePrefixTooltip = L10n.Tr(
            "Whether using a default prefix of screenshots filename or specifying it manually. Default value is agent name"
        );

        private static readonly string s_screenshotFilenamePrefix = L10n.Tr("Prefix");
        private static readonly string s_screenshotFilenamePrefixTooltip = L10n.Tr("Prefix of screenshots filename");
        private SerializedProperty _screenshotDefaultFilenamePrefixProp;
        private GUIContent _screenshotDefaultFilenamePrefixGUIContent;
        private SerializedProperty _screenshotFilenamePrefixProp;
        private GUIContent _screenshotFilenamePrefixGUIContent;

        private static readonly string s_screenshotSuperSize = L10n.Tr("Super Size");

        private static readonly string s_screenshotSuperSizeTooltip = L10n.Tr(
            "The factor to increase resolution with. Neither this nor Stereo Capture Mode can be specified"
        );

        private SerializedProperty _screenshotSuperSizeProp;
        private GUIContent _screenshotSuperSizeGUIContent;
        private GUIContent _screenshotSuperSizeGUIContentDisabled;

        private static readonly string s_screenshotStereoCaptureMode = L10n.Tr("Stereo Capture Mode");

        private static readonly string s_screenshotStereoCaptureModeTooltip =
            L10n.Tr(
                "The eye texture to capture when stereo rendering is enabled. Neither this nor Resolution Factor can be specified"
            );

        private SerializedProperty _screenshotStereoCaptureModeProp;
        private GUIContent _screenshotStereoCaptureModeGUIContent;
        private GUIContent _screenshotStereoCaptureModeGUIContentDisabled;

        private const int DefaultSuperSize = 1;

        private const ScreenCapture.StereoScreenCaptureMode DefaultStereoScreenCaptureMode =
            ScreenCapture.StereoScreenCaptureMode.LeftEye;

        // uGUI click and hold operator options
        private static readonly string s_uGUIClickAndHoldOperatorOptions = L10n.Tr("Click and Hold Operator Options");
        private static readonly string s_touchAndHoldDelayMillis = L10n.Tr("Click and Hold Millis");
        private static readonly string s_touchAndHoldDelayMillisTooltip = L10n.Tr("Delay time for click-and-hold [ms]");
        private SerializedProperty _touchAndHoldDelayMillisProp;
        private GUIContent _touchAndHoldDelayMillisGUIContent;

        // uGUI text input operator options
        private static readonly string s_uGUITextInputOperatorOptions = L10n.Tr("Text Input Operator Options");
        private static readonly string s_randomStringParams = L10n.Tr("Random String Parameters Table");
        private static readonly string s_randomStringParamsEntryKey = L10n.Tr("Game Object Name");
        private static readonly string s_randomStringParamsEntryKind = L10n.Tr("Characters Kind");
        private static readonly string s_randomStringParamsEntryMinLength = L10n.Tr("Minimum Length");
        private static readonly string s_randomStringParamsEntryMaxLength = L10n.Tr("Maximum Length");
        private SerializedProperty _randomStringParametersMapProp;
        private ReorderableList _randomStringParamsMapList;
        private const float Padding = 2f;
        private const int NumberOfLines = 4;

        private static readonly float s_elementHeight =
            EditorGUIUtility.singleLineHeight * NumberOfLines + Padding * (NumberOfLines + 1);

        private GUIContent _randomStringParamsEntryKeyGUIContent;
        private GUIContent _randomStringParamsEntryKindGUIContent;
        private GUIContent _randomStringParamsEntryMinLengthGUIContent;
        private GUIContent _randomStringParamsEntryMaxLengthGUIContent;

        private const float SpacerPixels = 10f;
        private const float SpacerPixelsUnderHeader = 4f;

        private void OnEnable()
        {
            Initialize();
        }


        private void Initialize()
        {
            _descriptionProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.description));
            _descriptionGUIContent = new GUIContent(s_description, s_descriptionTooltip);

            _lifespanProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.lifespanSec));
            _lifespanGUIContent = new GUIContent(s_lifespanSec, s_lifespanSecTooltip);

            _delayMillisProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.delayMillis));
            _delayMillisGUIContent = new GUIContent(s_delayMillis, s_delayMillisTooltip);

            _timeoutProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.secondsToErrorForNoInteractiveComponent));
            _timeoutGUIContent = new GUIContent(s_timeout, s_timeoutToolTip);

            _gizmosProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.gizmos));
            _gizmosGUIContent = new GUIContent(s_gizmos, s_gizmosTooltip);

            // Screenshot options
            _screenshotEnabledProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.screenshotEnabled));
            _screenshotEnabledGUIContent = new GUIContent(s_screenshotEnabled, s_screenshotEnabledTooltip);

            _screenshotDefaultFilenamePrefixProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.defaultScreenshotFilenamePrefix));
            _screenshotDefaultFilenamePrefixGUIContent = new GUIContent(
                s_screenshotDefaultFilenamePrefix,
                s_screenshotDefaultFilenamePrefixTooltip
            );
            _screenshotFilenamePrefixProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.screenshotFilenamePrefix));
            _screenshotFilenamePrefixGUIContent = new GUIContent(
                s_screenshotFilenamePrefix,
                s_screenshotFilenamePrefixTooltip
            );

            _screenshotSuperSizeProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.screenshotSuperSize));
            _screenshotSuperSizeGUIContent = new GUIContent(s_screenshotSuperSize, s_screenshotSuperSizeTooltip);
            _screenshotSuperSizeGUIContentDisabled = new GUIContent(DefaultSuperSize.ToString());

            _screenshotStereoCaptureModeProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.screenshotStereoCaptureMode));
            _screenshotStereoCaptureModeGUIContent = new GUIContent(
                s_screenshotStereoCaptureMode,
                s_screenshotStereoCaptureModeTooltip
            );
            _screenshotStereoCaptureModeGUIContentDisabled = new GUIContent(
                DefaultStereoScreenCaptureMode.ToString(),
                s_screenshotStereoCaptureModeTooltip
            );

            // uGUI click and hold operator options
            _touchAndHoldDelayMillisProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.touchAndHoldDelayMillis));
            _touchAndHoldDelayMillisGUIContent = new GUIContent(
                s_touchAndHoldDelayMillis,
                s_touchAndHoldDelayMillisTooltip
            );

            // uGUI text input operator options
            _randomStringParamsEntryKeyGUIContent = new GUIContent(s_randomStringParamsEntryKey);
            _randomStringParamsEntryKindGUIContent = new GUIContent(s_randomStringParamsEntryKind);
            _randomStringParamsEntryMinLengthGUIContent = new GUIContent(s_randomStringParamsEntryMinLength);
            _randomStringParamsEntryMaxLengthGUIContent = new GUIContent(s_randomStringParamsEntryMaxLength);
            _randomStringParametersMapProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.randomStringParametersMap));
            _randomStringParamsMapList = new ReorderableList(serializedObject, _randomStringParametersMapProp)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, s_randomStringParams),
                elementHeightCallback = _ => s_elementHeight,
                // XXX: Discarding parameters cannot be used on Unity 2019.x .
                // ReSharper disable UnusedParameter.Local
                drawElementCallback = (rect, index, isActive, isFocused) =>
                    // ReSharper enable UnusedParameter.Local
                {
                    var elemProp = _randomStringParametersMapProp.GetArrayElementAtIndex(index);
                    var rect1 = new Rect(rect) { y = rect.y + Padding, height = EditorGUIUtility.singleLineHeight };
                    EditorGUI.PropertyField(
                        rect1,
                        elemProp.FindPropertyRelative(
                            nameof(UGUIMonkeyAgent.RandomStringParametersEntry.GameObjectName)),
                        _randomStringParamsEntryKeyGUIContent
                    );

                    var rect2 = new Rect(rect)
                    {
                        y = rect1.y + EditorGUIUtility.singleLineHeight + Padding,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    EditorGUI.PropertyField(
                        rect2,
                        elemProp.FindPropertyRelative(
                            nameof(UGUIMonkeyAgent.RandomStringParametersEntry.CharactersKind)),
                        _randomStringParamsEntryKindGUIContent
                    );
                    var rect3 = new Rect(rect)
                    {
                        y = rect2.y + EditorGUIUtility.singleLineHeight + Padding,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    EditorGUI.PropertyField(
                        rect3,
                        elemProp.FindPropertyRelative(nameof(UGUIMonkeyAgent.RandomStringParametersEntry
                            .MinimumLength)),
                        _randomStringParamsEntryMinLengthGUIContent
                    );
                    var rect4 = new Rect(rect)
                    {
                        y = rect3.y + EditorGUIUtility.singleLineHeight + Padding,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    EditorGUI.PropertyField(
                        rect4,
                        elemProp.FindPropertyRelative(nameof(UGUIMonkeyAgent.RandomStringParametersEntry
                            .MaximumLength)),
                        _randomStringParamsEntryMaxLengthGUIContent
                    );
                }
            };
        }


        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionGUIContent);
            EditorGUILayout.PropertyField(_lifespanProp, _lifespanGUIContent);
            EditorGUILayout.PropertyField(_delayMillisProp, _delayMillisGUIContent);
            EditorGUILayout.PropertyField(_timeoutProp, _timeoutGUIContent);
            EditorGUILayout.PropertyField(_gizmosProp, _gizmosGUIContent);

            // Screenshot options
            DrawHeader(s_screenshotOptions);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_screenshotEnabledProp, _screenshotEnabledGUIContent);
                if (_screenshotEnabledProp.boolValue)
                {
                    EditorGUILayout.LabelField(s_screenshotFilename, EditorStyles.boldLabel);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(
                            _screenshotDefaultFilenamePrefixProp,
                            _screenshotDefaultFilenamePrefixGUIContent
                        );
                        if (!_screenshotDefaultFilenamePrefixProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(
                                _screenshotFilenamePrefixProp,
                                _screenshotFilenamePrefixGUIContent
                            );
                        }
                    }

                    var screenCaptureMode =
                        (ScreenCapture.StereoScreenCaptureMode)_screenshotStereoCaptureModeProp.enumValueIndex;
                    if (screenCaptureMode == DefaultStereoScreenCaptureMode)
                    {
                        _screenshotSuperSizeProp.intValue = EditorGUILayout.IntSlider(
                            _screenshotSuperSizeGUIContent,
                            _screenshotSuperSizeProp.intValue,
                            1,
                            100
                        );
                    }
                    else
                    {
                        EditorGUILayout.LabelField(
                            _screenshotSuperSizeGUIContent,
                            _screenshotSuperSizeGUIContentDisabled
                        );
                    }

                    if (_screenshotSuperSizeProp.intValue == DefaultSuperSize)
                    {
                        var screenshotStereoCaptureMode =
                            (ScreenCapture.StereoScreenCaptureMode)EditorGUILayout.EnumPopup(
                                _screenshotStereoCaptureModeGUIContent,
                                (ScreenCapture.StereoScreenCaptureMode)_screenshotStereoCaptureModeProp
                                    .enumValueIndex
                            );
                        _screenshotStereoCaptureModeProp.enumValueIndex = Clamp(
                            (int)screenshotStereoCaptureMode,
                            1,
                            3
                        );
                    }
                    else
                    {
                        EditorGUILayout.LabelField(
                            _screenshotStereoCaptureModeGUIContent,
                            _screenshotStereoCaptureModeGUIContentDisabled
                        );
                    }
                }
            }

            // uGUI click and hold operator options
            DrawHeader(s_uGUIClickAndHoldOperatorOptions);
            EditorGUILayout.PropertyField(_touchAndHoldDelayMillisProp, _touchAndHoldDelayMillisGUIContent);

            // uGUI text input operator options
            DrawHeader(s_uGUITextInputOperatorOptions);
            _randomStringParamsMapList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawHeader(string label)
        {
            GUILayout.Space(SpacerPixels);
            GUILayout.Label(label, EditorStyles.boldLabel);
            GUILayout.Space(SpacerPixelsUnderHeader);
        }

        // XXX: Fallback for Unity 2019.x
        private static int Clamp(int value, int min, int max) => value < min ? min : max < value ? max : value;
    }
}
