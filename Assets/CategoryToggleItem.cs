using System;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;
using TMPro;

public class CategoryToggleItem : MonoBehaviour
{
    public Button button;            // The button component on prefab root
    public LeanToggle toggle;        // LeanToggle child
    public Image backgroundImage;
    public Image categoryIconImage;
    public TMP_Text labelText;       // Main label (customizable)
    public TMP_Text descriptionText; // New description label

    public PrefCategory PrefCategory { get; private set; }

    // Event fired when toggle changes state
    public event Action<bool> OnToggleChanged;

    private void Awake()
    {
        if (button != null && toggle != null)
        {
            // Sync toggle on button click
            button.onClick.AddListener(() =>
            {
                bool newState = !toggle.On;
                toggle.Set(newState);
                OnToggleChanged?.Invoke(newState);
            });
        }
    }

    public void Setup(
        string name,
        string description,
        PrefCategory prefCategory,
        int categoryIconIndex,
        Sprite[] categoryIcons,
        Sprite spriteSolo,
        Sprite spriteFirst,
        Sprite spriteMiddle,
        Sprite spriteLast,
        int index,
        int totalCount)
    {
        PrefCategory = prefCategory;

        if (labelText != null)
        {
            // DEBUG LOG: Uncomment if you want to check
            // Debug.Log($"Setting labelText to: {name}");
            labelText.text = name;
        }

        if (descriptionText != null)
            descriptionText.text = description;

        if (categoryIconImage != null && categoryIcons != null && categoryIconIndex >= 0 && categoryIconIndex < categoryIcons.Length)
            categoryIconImage.sprite = categoryIcons[categoryIconIndex];

        if (backgroundImage != null)
        {
            if (totalCount == 1)
                backgroundImage.sprite = spriteSolo;
            else if (index == 0)
                backgroundImage.sprite = spriteFirst;
            else if (index == totalCount - 1)
                backgroundImage.sprite = spriteLast;
            else
                backgroundImage.sprite = spriteMiddle;
        }

        if (toggle != null)
        {
            toggle.OnOn.RemoveAllListeners();
            toggle.OnOff.RemoveAllListeners();

            toggle.OnOn.AddListener(() => OnToggleChanged?.Invoke(true));
            toggle.OnOff.AddListener(() => OnToggleChanged?.Invoke(false));

            toggle.Set(false);
        }
    }

    public bool IsOn()
    {
        return toggle != null && toggle.On;
    }

    public void SetOn(bool value)
    {
        if (toggle != null)
            toggle.Set(value);
    }

    public void SetInteractable(bool value)
    {
        if (toggle != null)
            toggle.enabled = value;

        if (button != null)
            button.interactable = value;
    }
}
