// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Strategies;
using Unity.RecordedPlayback;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// This is an Agent that playback uGUI operations with the Recorded Playback feature of the Automated QA package.
    /// <see href="https://docs.unity3d.com/Packages/com.unity.automated-testing@latest"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New UGUIPlaybackAgent", menuName = "Anjin/uGUI Playback Agent", order = 31)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase")]
    public class UGUIPlaybackAgent : AbstractAgent
    {
        /// <summary>
        /// JSON file recorded by AutomatedQA package
        /// </summary>
        public TextAsset recordedJson;

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            try
            {
                Logger.Log($"Enter {this.name}.Run()");

                if (recordedJson == null)
                {
                    throw new NullReferenceException("recordingJson is null");
                }

                await Play(recordedJson, token);
                // Note: If playback is not possible, AQA will output a LogError and exit. You must handle LogError with the ErrorHandlerAgent.
            }
            finally
            {
                try
                {
                    var screenshotStorePath = Path.Combine(
                        AutopilotState.Instance.settings!.ScreenshotsPath,
                        new TwoTieredCounterStrategy(this.name).GetFilename().Replace(".png", ""));
                    if (!Directory.Exists(screenshotStorePath))
                    {
                        Directory.CreateDirectory(screenshotStorePath);
                    }

                    StoreScreenshots(screenshotStorePath);
                    Logger.Log($"Stored screenshots to {screenshotStorePath}");
                }
                finally
                {
                    Logger.Log($"Exit {this.name}.Run()");
                }
            }
        }

        private static void StoreScreenshots(string dst)
        {
            var aqaPath = Path.Combine(Application.persistentDataPath, "AutomatedQA");
            var aqaDir = new DirectoryInfo(aqaPath);
            var src = aqaDir.EnumerateDirectories()
                .Where(x => x.Name.StartsWith("Playback screenshots"))
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();
            if (src == null)
            {
                return;
            }

            foreach (var file in src.GetFiles())
            {
                File.Move(file.FullName, Path.Combine(dst, file.Name));
            }
        }

        private static IEnumerator Play(TextAsset recordingJson, CancellationToken token)
        {
            if (RecordedPlaybackController.Exists())
            {
                RecordedPlaybackController.Instance.Reset();
            }

            RecordedPlaybackPersistentData.SetRecordingMode(RecordingMode.Playback);
            RecordedPlaybackPersistentData.SetRecordingData(recordingJson.text);
            RecordedPlaybackController.Instance.Begin();

            while (RecordedPlaybackController.Exists())
                // Use the fact that RecordedPlaybackController._instance becomes null when playback is finished.
                // This behavior is from Automated QA v0.8.1. May change in the future
            {
                token.ThrowIfCancellationRequested();
                yield return null;
            }
            // Note: Driver.Perform.PlayRecording()を使わない理由は以下
            //   1. 内部的に `loadEntryScene = true` のRecordedPlaybackAutomatorになるため、Sceneがロードされてしまう
            //   2. 引数がJSONファイルパスだが、TextAssetのraw dataのほうが都合がよい
            //   3. RecordedPlaybackAutomatorがRecordedPlaybackControllerの終了を待つのに使っているIsPlaybackCompleted()にバグがあり、無限ループする
            // 3の理由から、Unity.RecordedPlayback.RecordedPlaybackAutomatorも使用せず、直接RecordedPlaybackControllerを操作している
        }
    }
}
