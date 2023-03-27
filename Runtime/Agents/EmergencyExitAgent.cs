// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Search and click <c>EmergencyExit</c> annotation component.
    /// This agent will not be terminate and will continue to be observe.
    /// </summary>
    [CreateAssetMenu(fileName = "New EmergencyExitAgent", menuName = "Anjin/Emergency Exit Agent", order = 21)]
    public class EmergencyExitAgent : AbstractAgent
    {
        /// <inheritdoc />
        [SuppressMessage("Blocker Bug", "S2190:Recursion should not be infinite")]
        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            var selectables = new Selectable[20];
            while (true)
            {
                var allSelectableCount = Selectable.allSelectableCount;
                if (selectables.Length < allSelectableCount)
                {
                    selectables = new Selectable[allSelectableCount];
                }

                Selectable.AllSelectablesNoAlloc(selectables);
                for (var i = 0; i < allSelectableCount; i++)
                {
                    if (selectables[i].TryGetComponent<EmergencyExit>(out var emergencyExit))
                    {
                        ClickEmergencyExitButton(emergencyExit);
                    }
                }

                await UniTask.NextFrame(token);
            }

            // Note: This agent is not terminate myself
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void ClickEmergencyExitButton(EmergencyExit emergencyExit)
        {
            Logger.Log($"Click emergency exit button: {emergencyExit.name}");

            var gameObject = emergencyExit.gameObject;
            var button = gameObject.GetComponent<Button>();
            button.OnPointerClick(new PointerEventData(EventSystem.current));
            // Note: Button.OnPointerClick() does not look at PointerEventData coordinates.
        }
    }
}
