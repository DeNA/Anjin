// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Strategies;
using TestHelper.Monkey;
using TestHelper.Monkey.Annotations.Enums;
using TestHelper.Monkey.Exceptions;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.Random;
using TestHelper.Random;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// This is an Agent that randomly manipulates uGUI components.
    /// </summary>
    [CreateAssetMenu(fileName = "New UGUIMonkeyAgent", menuName = "Anjin/uGUI Monkey Agent", order = 30)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase")]
    public class UGUIMonkeyAgent : AbstractAgent
    {
        /// <summary>
        /// Agent running lifespan [sec]. When specified zero, so unlimited running
        /// </summary>
        public long lifespanSec;

        /// <summary>
        /// Delay time between random operations [ms]
        /// </summary>
        public int delayMillis = 200;

        /// <summary>
        /// No-element timeout [sec].
        /// Abort Autopilot when the interactable UI/ 2D/ 3D element does not appear for the specified seconds.
        /// Disable detection if set to 0.
        /// </summary>
        public int secondsToErrorForNoInteractiveComponent = 5;

        /// <summary>
        /// Repeating operation detection buffer length.
        /// Abort Autopilot when repeating operation is detected within the specified buffer length.
        /// For example, if the buffer length is 10, repeating 5-step sequences can be detected.
        /// Disable detection if set to 0.
        /// </summary>
        public int bufferLengthForDetectLooping = 10;

        /// <summary>
        /// Delay time for click-and-hold
        /// </summary>
        public int touchAndHoldDelayMillis = 1000;

        /// <summary>
        /// Show Gizmos on <c>GameView</c> during running monkey test if true
        /// </summary>
        public bool gizmos;

        /// <summary>
        /// Name-based random string parameters strategy. If a GameObject's name is match to the key, the random string
        /// parameters will be used. If multiple entries have a same key, which entry get used is unspecified.
        /// Otherwise <c cref="RandomStringParameters.Default" /> will be used.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<RandomStringParametersEntry> randomStringParametersMap =
            new List<RandomStringParametersEntry>();

        /// <summary>
        /// Whether screenshot is enabled
        /// </summary>
        public bool screenshotEnabled;

        /// <summary>
        /// Whether using a default directory or specifying manually
        /// </summary>
        [Obsolete("Uses AutopilotSettings.ScreenshotsPath")]
        public bool defaultScreenshotDirectory = true;

        /// <summary>
        /// Directory to save screenshots.
        /// If omitted, the directory specified by command line argument "-testHelperScreenshotDirectory" is used.
        /// If the command line argument is also omitted, <c>Application.persistentDataPath</c> + "/TestHelper/Screenshots/" is used.
        /// </summary>
        [Obsolete("Uses AutopilotSettings.ScreenshotsPath")]
        public string screenshotDirectory;

        /// <summary>
        /// Whether using a default file name prefix or specifying manually
        /// </summary>
        public bool defaultScreenshotFilenamePrefix = true;

        /// <summary>
        /// File name prefix for screenshot images. If the value is null or empty, a default value will be used.
        /// The default value is the current test name if the agent is running on tests. Otherwise, be a caller method
        /// name
        /// </summary>
        public string screenshotFilenamePrefix;

        /// <inheritdoc cref="ScreenshotOptions.SuperSize" />
        public int screenshotSuperSize = 1;

        /// <inheritdoc cref="ScreenshotOptions.StereoCaptureMode" />
        public ScreenCapture.StereoScreenCaptureMode screenshotStereoCaptureMode =
            ScreenCapture.StereoScreenCaptureMode.LeftEye;

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken cancellationToken)
        {
            Logger.Log($"Enter {this.name}.Run()");

            var random = new RandomWrapper(this.Random.Next());
            var config = new MonkeyConfig
            {
                Lifetime = lifespanSec > 0 ? TimeSpan.FromSeconds(lifespanSec) : TimeSpan.MaxValue,
                DelayMillis = delayMillis,
                Random = random,
                Logger = Logger,
                SecondsToErrorForNoInteractiveComponent = secondsToErrorForNoInteractiveComponent,
                BufferLengthForDetectLooping = bufferLengthForDetectLooping,
                Gizmos = gizmos,
                Screenshots = screenshotEnabled
                    ? new ScreenshotOptions
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        Directory = AutopilotState.Instance.settings.ScreenshotsPath,
                        FilenameStrategy = new TwoTieredCounterStrategy(
                            defaultScreenshotFilenamePrefix ? this.name : screenshotFilenamePrefix
                        ),
                        SuperSize = screenshotSuperSize,
                        StereoCaptureMode = screenshotStereoCaptureMode
                    }
                    : null,
                Operators = new IOperator[]
                {
                    new UGUIClickOperator(), //
                    new UGUIClickAndHoldOperator(holdMillis: touchAndHoldDelayMillis), //
                    new UGUITextInputOperator(
                        randomStringParams: GetRandomStringParameters,
                        randomString: new RandomStringImpl(random)),
                },
            };

            try
            {
                await Monkey.Run(config, cancellationToken);
            }
            catch (TimeoutException e)
            {
                var message = $"{e.GetType().Name}: {e.Message}";
                Logger.Log(message);

                // ReSharper disable once MethodSupportsCancellation; Do not use this Agent's CancellationToken.
                AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, message, e.StackTrace).Forget();
            }
            catch (InfiniteLoopException e)
            {
                var message = $"{e.GetType().Name}: {e.Message}";
                Logger.Log(message);

                // ReSharper disable once MethodSupportsCancellation; Do not use this Agent's CancellationToken.
                AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, message, e.StackTrace).Forget();
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }

        private RandomStringParameters GetRandomStringParameters(GameObject gameObject)
        {
            // NOTE: Random string parameters maps not get too large, so linear searching allowed in here.
            for (var i = randomStringParametersMap.Count - 1; 0 <= i; i--)
            {
                var entry = randomStringParametersMap[i];
                if (gameObject.name == entry.GameObjectName)
                {
                    return entry.AsRandomStringParameters();
                }
            }

            return RandomStringParameters.Default;
        }

        /// <summary>
        /// Serializable struct for <c cref="RandomStringParameters" />
        /// </summary>
        [Serializable]
        public struct RandomStringParametersEntry
        {
            /// <summary>
            /// Name of GameObjects.
            /// </summary>
            public string GameObjectName;

            /// <inheritdoc cref="RandomStringParameters.MinimumLength" />
            public int MinimumLength;

            /// <inheritdoc cref="RandomStringParameters.MaximumLength" />
            public int MaximumLength;

            /// <inheritdoc cref="RandomStringParameters.CharactersKind" />
            public CharactersKind CharactersKind;

            /// <summary>
            /// Returns a <c cref="RandomStringParameters" />
            /// </summary>
            /// <returns></returns>
            public RandomStringParameters AsRandomStringParameters() =>
                new RandomStringParameters(MinimumLength, MaximumLength, CharactersKind);
        }
    }
}
