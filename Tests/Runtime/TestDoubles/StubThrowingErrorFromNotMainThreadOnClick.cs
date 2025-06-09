// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// A test stub throw errors when clicked
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class StubThrowingErrorFromNotMainThreadOnClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Task.Run(() => Debug.LogException(new Exception($"TEST on thread #{Thread.CurrentThread.ManagedThreadId}"))
            );
        }
    }
}
