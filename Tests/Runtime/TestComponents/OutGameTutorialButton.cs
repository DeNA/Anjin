// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace DeNA.Anjin.TestComponents
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("")] // Hide from "Add Component" picker
    public class OutGameTutorialButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Debug.Log($"{gameObject.name} Clicked!");

            var button = GetComponent<Button>();
            button.interactable = false;

            var nextButtonNumber = int.Parse(gameObject.name.Replace("Button", "")) + 1;
            var nextButton = GameObject.Find($"Button{nextButtonNumber}");
            if (nextButton != null)
            {
                nextButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                Debug.Log("Tutorial Completed!");
            }
        }
    }
}
