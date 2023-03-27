// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace DeNA.Anjin.TestDoubles
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("")]
    public class SpyButton : MonoBehaviour
    {
        public bool IsClicked { get; private set; }

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            IsClicked = true;
        }
    }
}
