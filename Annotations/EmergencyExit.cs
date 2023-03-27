// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace DeNA.Anjin.Annotations
{
    /// <summary>
    /// Attach this component to the "Return to Title Scene" button or each other buttons that are displayed in the event of communication errors.
    /// As soon as the component appears on the screen, it is click by <c>EmergencyExitAgent</c>
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EmergencyExit : MonoBehaviour
    {
    }
}
