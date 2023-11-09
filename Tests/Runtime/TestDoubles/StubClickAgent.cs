// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeNA.Anjin.TestDoubles
{
    public class StubClickAgent : AbstractAgent
    {
        [SerializeField] public string targetName;

        public override UniTask Run(CancellationToken token)
        {
            foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name != targetName)
                {
                    continue;
                }

                var target = obj;
                if (!target.TryGetComponent<IPointerClickHandler>(out var handler))
                {
                    Debug.LogWarning(
                        $"{target.name} did not have any components that derived {nameof(IPointerClickHandler)}"
                    );
                    continue;
                }

                handler.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            return UniTask.CompletedTask;
        }
    }
}
