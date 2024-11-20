﻿// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using DeNA.Anjin.Strategies;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.ScreenshotFilenameStrategies;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.UI;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Wait for the <c>EmergencyExitAnnotation</c> component to appear and click attached <c>UnityEngine.UI.Button</c>.
    /// This Agent will not be terminated and will continue to be observed.
    /// </summary>
    [CreateAssetMenu(fileName = "New UGUIEmergencyExitAgent", menuName = "Anjin/uGUI Emergency Exit Agent", order = 51)]
    public class UGUIEmergencyExitAgent : AbstractAgent
    {
        /// <summary>
        /// Interval in milliseconds to check for the appearance of the <c>EmergencyExitAnnotation</c> component.
        /// </summary>
        public int intervalMillis = 1000;

        /// <summary>
        /// Take a screenshot when click the EmergencyExit button.
        /// </summary>
        public bool screenshot;

        private static IClickOperator ClickOperator => new UGUIClickOperator();
        private IScreenshotFilenameStrategy _filenameStrategy;

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");
            this._filenameStrategy = new TwoTieredCounterStrategy(this.name);

            try
            {
                var selectables = new Selectable[20];

                while (true) // Note: This agent is not terminate myself
                {
                    var allSelectableCount = Selectable.allSelectableCount;
                    if (selectables.Length < allSelectableCount)
                    {
                        selectables = new Selectable[allSelectableCount];
                    }

                    Selectable.AllSelectablesNoAlloc(selectables);
                    for (var i = 0; i < allSelectableCount; i++)
                    {
                        if (selectables[i].TryGetComponent<EmergencyExitAnnotation>(out var emergencyExit))
                        {
                            await ClickEmergencyExitButton(emergencyExit, token);
                        }
                    }

                    await UniTask.Delay(intervalMillis, ignoreTimeScale: true, cancellationToken: token);
                }
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }

        private async UniTask ClickEmergencyExitButton(EmergencyExitAnnotation emergencyExit,
            CancellationToken cancellationToken = default)
        {
            var button = emergencyExit.gameObject.GetComponent<Button>();
            if (!ClickOperator.CanOperate(button))
            {
                return;
            }

            var message = new StringBuilder($"Click emergency exit button: {button.gameObject.name}");
            if (screenshot)
            {
                var filename = _filenameStrategy.GetFilename();
                await ScreenshotHelper.TakeScreenshot(filename: filename).ToUniTask(button);
                message.Append($" ({filename})");
            }

            Logger.Log(message.ToString());

            await ClickOperator.OperateAsync(button, cancellationToken);
        }
    }
}