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
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this agent instance");
        private SerializedProperty _descriptionProp;
        private GUIContent _descriptionGUIContent;

        private static readonly string s_lifespanSec = L10n.Tr("Lifespan Sec");

        private static readonly string s_lifespanSecTooltip =
            L10n.Tr("Agent running lifespan [sec]. When specified zero, so unlimited running");

        private SerializedProperty _lifespanProp;
        private GUIContent _lifespanGUIContent;

        private static readonly string s_delayMillis = L10n.Tr("Delay Millis");
        private static readonly string s_delayMillisTooltip = L10n.Tr("Delay time between random operations [ms]");
        private SerializedProperty _delayMillisProp;
        private GUIContent _delayMillisGUIContent;

        private static readonly string s_timeout =
            L10n.Tr("Secs Searching Components");

        private static readonly string s_timeoutToolTip = L10n.Tr(
            "Seconds to determine that an error has occurred when an object that can be interacted with does not exist"
        );

        private SerializedProperty _timeoutProp;
        private GUIContent _timeoutGUIContent;

        private static readonly string s_touchAndHoldDelayMillis = L10n.Tr("Touch and Hold Millis");
        private static readonly string s_touchAndHoldDelayMillisTooltip = L10n.Tr("Delay time for touch-and-hold [ms]");
        private SerializedProperty _touchAndHoldDelayMillisProp;
        private GUIContent _touchAndHoldDelayMillisGUIContent;

        private static readonly string s_gizmos = L10n.Tr("Enable Gizmos");

        private static readonly string s_gizmosTooltip =
            L10n.Tr("Show Gizmos on GameView during running monkey test if true");

        private SerializedProperty _gizmosProp;
        private GUIContent _gizmosGUIContent;

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

        private static readonly string s_screenshotOptions = L10n.Tr("Screenshot Options");
        private static readonly string s_screenshotEnabled = L10n.Tr("Enabled");
        private static readonly string s_screenshotEnabledTooltip = L10n.Tr("Whether screenshot is enabled or not");
        private SerializedProperty _screenshotEnabledProp;
        private GUIContent _screenshotEnabledGUIContent;

        private static readonly string s_screenshotDirectory = L10n.Tr("Directory");
        private static readonly string s_screenshotDefaultDirectory = L10n.Tr("Use Default");

        private static readonly string s_screenshotDefaultDirectoryTooltip = L10n.Tr(
            @"Whether using a default directory path to save screenshots or specifying it manually. Default value is specified by command line argument ""-testHelperScreenshotDirectory"". If the command line argument is also omitted, Application.persistentDataPath + ""/TestHelper/Screenshots/"" is used."
        );

        private static readonly string s_screenshotDirectoryPath = L10n.Tr("Path");
        private static readonly string s_screenshotDirectoryPathTooltip = L10n.Tr("Directory path to save screenshots");
        private SerializedProperty _screenshotDefaultDirectoryProp;
        private GUIContent _screenshotDefaultDirectoryGUIContent;
        private SerializedProperty _screenshotDirectoryProp;
        private GUIContent _screenshotDirectoryGUIContent;

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

            _touchAndHoldDelayMillisProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.touchAndHoldDelayMillis));
            _touchAndHoldDelayMillisGUIContent = new GUIContent(
                s_touchAndHoldDelayMillis,
                s_touchAndHoldDelayMillisTooltip
            );

            _gizmosProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.gizmos));
            _gizmosGUIContent = new GUIContent(s_gizmos, s_gizmosTooltip);

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
                        elemProp.FindPropertyRelative(nameof(UGUIMonkeyAgent.RandomStringParametersEntry.GameObjectName)),
                        _randomStringParamsEntryKeyGUIContent
                    );

                    var rect2 = new Rect(rect)
                    {
                        y = rect1.y + EditorGUIUtility.singleLineHeight + Padding,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    EditorGUI.PropertyField(
                        rect2,
                        elemProp.FindPropertyRelative(nameof(UGUIMonkeyAgent.RandomStringParametersEntry.CharactersKind)),
                        _randomStringParamsEntryKindGUIContent
                    );
                    var rect3 = new Rect(rect)
                    {
                        y = rect2.y + EditorGUIUtility.singleLineHeight + Padding,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    EditorGUI.PropertyField(
                        rect3,
                        elemProp.FindPropertyRelative(nameof(UGUIMonkeyAgent.RandomStringParametersEntry.MinimumLength)),
                        _randomStringParamsEntryMinLengthGUIContent
                    );
                    var rect4 = new Rect(rect)
                    {
                        y = rect3.y + EditorGUIUtility.singleLineHeight + Padding,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    EditorGUI.PropertyField(
                        rect4,
                        elemProp.FindPropertyRelative(nameof(UGUIMonkeyAgent.RandomStringParametersEntry.MaximumLength)),
                        _randomStringParamsEntryMaxLengthGUIContent
                    );
                }
            };

            _screenshotEnabledProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.screenshotEnabled));
            _screenshotEnabledGUIContent = new GUIContent(s_screenshotEnabled, s_screenshotEnabledTooltip);

            _screenshotDefaultDirectoryProp =
                serializedObject.FindProperty(nameof(UGUIMonkeyAgent.defaultScreenshotDirectory));
            _screenshotDefaultDirectoryGUIContent = new GUIContent(
                s_screenshotDefaultDirectory,
                s_screenshotDefaultDirectoryTooltip
            );
            _screenshotDirectoryProp = serializedObject.FindProperty(nameof(UGUIMonkeyAgent.screenshotDirectory));
            _screenshotDirectoryGUIContent = new GUIContent(
                s_screenshotDirectoryPath,
                s_screenshotDirectoryPathTooltip
            );

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
        }


        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionGUIContent);
            EditorGUILayout.PropertyField(_lifespanProp, _lifespanGUIContent);
            EditorGUILayout.PropertyField(_delayMillisProp, _delayMillisGUIContent);
            EditorGUILayout.PropertyField(_timeoutProp, _timeoutGUIContent);
            EditorGUILayout.PropertyField(_touchAndHoldDelayMillisProp, _touchAndHoldDelayMillisGUIContent);
            EditorGUILayout.PropertyField(_gizmosProp, _gizmosGUIContent);
            _randomStringParamsMapList.DoLayoutList();
            EditorGUILayout.LabelField(s_screenshotOptions, EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_screenshotEnabledProp, _screenshotEnabledGUIContent);
                if (_screenshotEnabledProp.boolValue)
                {
                    EditorGUILayout.LabelField(s_screenshotDirectory, EditorStyles.boldLabel);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(
                            _screenshotDefaultDirectoryProp,
                            _screenshotDefaultDirectoryGUIContent
                        );
                        if (!_screenshotDefaultDirectoryProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(_screenshotDirectoryProp, _screenshotDirectoryGUIContent);
                        }
                    }

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

            serializedObject.ApplyModifiedProperties();
        }


        // XXX: Fallback for Unity 2019.x
        private static int Clamp(int value, int min, int max) => value < min ? min : max < value ? max : value;
    }
}
