// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// A test stub throw errors when clicked
    /// </summary>
    [AddComponentMenu("")]
    public class StubThrowingErrorFromMainThreadOnClick : MonoBehaviour, IPointerClickHandler
    {
        public async void OnPointerClick(PointerEventData eventData)
        {
            await UniTask.SwitchToMainThread();
            throw new Exception($"TEST on thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
