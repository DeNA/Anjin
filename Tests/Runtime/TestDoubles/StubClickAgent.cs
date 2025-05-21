// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// A test double for agent. This agent immediately click game objects that have the name specified
    /// </summary>
    public class StubClickAgent : AbstractAgent
    {
        /// <summary>
        /// A name of game objects to click
        /// </summary>
        [SerializeField]
        public string targetName;


        /// <inheritdoc />
        public override UniTask Run(CancellationToken token)
        {
            foreach (var obj in SceneManager.GetActiveScene().GetRootGameObjects())
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
