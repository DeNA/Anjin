// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
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

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            var selectables = new Selectable[20];
            try
            {
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
                            ClickEmergencyExitButton(emergencyExit);
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

        private void ClickEmergencyExitButton(EmergencyExitAnnotation emergencyExitAnnotation)
        {
            var gameObject = emergencyExitAnnotation.gameObject;
            Logger.Log($"Click emergency exit button: {gameObject.name}");

            var button = gameObject.GetComponent<Button>();
            button.OnPointerClick(new PointerEventData(EventSystem.current));
            // Note: Button.OnPointerClick() does not look at PointerEventData coordinates.
        }
    }
}
