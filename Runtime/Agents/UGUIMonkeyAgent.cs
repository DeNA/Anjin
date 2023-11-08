// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey;
using TestHelper.Monkey.Annotations.Enums;
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
        /// Seconds to determine that an error has occurred when an object that can be interacted with does not exist
        /// </summary>
        public int secondsToErrorForNoInteractiveComponent = 5;

        /// <summary>
        /// Delay time for touch-and-hold
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
        public bool defaultScreenshotDirectory = true;

        /// <inheritdoc cref="ScreenshotOptions.Directory" />
        public string screenshotDirectory;

        /// <summary>
        /// Whether using a default file name prefix or specifying manually
        /// </summary>
        public bool defaultScreenshotFilenamePrefix = true;

        /// <inheritdoc cref="ScreenshotOptions.FilenamePrefix" />
        public string screenshotFilenamePrefix;

        /// <inheritdoc cref="ScreenshotOptions.SuperSize" />
        public int screenshotSuperSize = 1;

        /// <inheritdoc cref="ScreenshotOptions.StereoCaptureMode" />
        public ScreenCapture.StereoScreenCaptureMode screenshotStereoCaptureMode =
            ScreenCapture.StereoScreenCaptureMode.LeftEye;

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            var random = new RandomImpl(this.Random.Next());
            var config = new MonkeyConfig
            {
                Lifetime = lifespanSec > 0 ? TimeSpan.FromSeconds(lifespanSec) : TimeSpan.MaxValue,
                DelayMillis = delayMillis,
                Random = random,
                RandomString = new RandomStringImpl(random),
                RandomStringParametersStrategy = GetRandomStringParameters,
                SecondsToErrorForNoInteractiveComponent = secondsToErrorForNoInteractiveComponent,
                TouchAndHoldDelayMillis = touchAndHoldDelayMillis,
                Gizmos = gizmos,
                Screenshots = screenshotEnabled
                    ? new ScreenshotOptions
                    {
                        Directory = defaultScreenshotDirectory ? null : screenshotDirectory,
                        FilenamePrefix = defaultScreenshotFilenamePrefix ? null : screenshotFilenamePrefix,
                        SuperSize = screenshotSuperSize,
                        StereoCaptureMode = screenshotStereoCaptureMode
                    }
                    : null
            };
            await Monkey.Run(config, token);

            Logger.Log($"Exit {this.name}.Run()");
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
