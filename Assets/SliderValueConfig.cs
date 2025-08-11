using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;

[System.Serializable]
public class SliderGroup
{
    [Header("Accordion")]
    public Button mainButton;
    public GameObject settingsPanel;
    public Sprite spriteClosed;
    public Sprite spriteOpened;

    [Header("Fields")]
    public TMP_InputField minValueField;
    public TMP_InputField maxValueField;
    public TMP_InputField startValueField;

    [Header("Direction radio (index 0 = Off, index 1 = On)")]
    public List<LeanToggle> directionOptions; // Expect size = 2

    // PlayerPrefs keys (auto-assigned)
    [HideInInspector] public string minKey;
    [HideInInspector] public string maxKey;
    [HideInInspector] public string startKey;
    [HideInInspector] public string flipKey;

    // Runtime state
    [HideInInspector] public int currentDirectionIndex; // 0 = Off, 1 = On
}

public class SliderValueConfig : MonoBehaviour
{
    public GameObject sliderConfigPanel;

    [Header("Core")]
    public RobotArmSelection robotArmSelection;
    public List<SliderGroup> sliderGroups = new List<SliderGroup>();

    [Header("Open/Close values (fields)")]
    public TMP_InputField openButtonValueIn;
    public TMP_InputField closeButtonValueIn;

    [Header("Buttons group (accordion)")]
    public Button buttonsHeaderButton;
    public GameObject buttonsSettingsPanel;
    public Sprite buttonsSpriteClosed;
    public Sprite buttonsSpriteOpened;

    // Keys for open/close
    [HideInInspector] public string openKey;
    [HideInInspector] public string closeKey;

    // Guard against recursive radio events
    private bool isProgrammatic;

    void Awake()
    {
        // Auto-assign keys for slider groups
        for (int i = 0; i < sliderGroups.Count; i++)
        {
            int n = i + 1;
            sliderGroups[i].minKey = $"Slider{n}_Min";
            sliderGroups[i].maxKey = $"Slider{n}_Max";
            sliderGroups[i].startKey = $"Slider{n}_Start";
            sliderGroups[i].flipKey = $"Slider{n}_FlipDirection";
        }

        // Keys for open/close (as requested)
        openKey = "OpenButtonPressed";
        closeKey = "CloseButtonPressed";
    }

    void Start()
    {
        // Setup slider groups (accordion + load + hooks)
        foreach (var group in sliderGroups)
        {
            // Start collapsed and set header sprite closed
            if (group.settingsPanel != null) group.settingsPanel.SetActive(false);

            if (group.mainButton != null)
            {
                var img = group.mainButton.GetComponent<Image>();
                if (img != null && group.spriteClosed != null)
                    img.sprite = group.spriteClosed;

                group.mainButton.onClick.AddListener(() => ToggleGroup(group));
            }

            LoadSliderGroup(group);
            HookAutoSave(group);
            HookDirectionRadios(group);
        }

        // Setup Buttons group accordion
        if (buttonsSettingsPanel != null)
            buttonsSettingsPanel.SetActive(false);

        if (buttonsHeaderButton != null)
        {
            var img = buttonsHeaderButton.GetComponent<Image>();
            if (img != null && buttonsSpriteClosed != null)
                img.sprite = buttonsSpriteClosed;

            buttonsHeaderButton.onClick.AddListener(ToggleButtonsAccordion);
        }

        // Load button open/close values
        int open = PlayerPrefs.GetInt(openKey, 105);
        int close = PlayerPrefs.GetInt(closeKey, 177);
        robotArmSelection?.ConfigureOpenCloseValues(open, close);

        if (openButtonValueIn) openButtonValueIn.text = open.ToString();
        if (closeButtonValueIn) closeButtonValueIn.text = close.ToString();

        if (openButtonValueIn) openButtonValueIn.onEndEdit.AddListener(_ => SaveButtonValues());
        if (closeButtonValueIn) closeButtonValueIn.onEndEdit.AddListener(_ => SaveButtonValues());
    }

    // Accordion for slider groups
    void ToggleGroup(SliderGroup group)
    {
        if (group.settingsPanel == null || group.mainButton == null) return;

        bool isActive = group.settingsPanel.activeSelf;
        group.settingsPanel.SetActive(!isActive);

        var img = group.mainButton.GetComponent<Image>();
        if (img != null)
            img.sprite = isActive ? group.spriteClosed : group.spriteOpened;
    }

    // Accordion for Buttons group
    void ToggleButtonsAccordion()
    {
        if (buttonsSettingsPanel == null || buttonsHeaderButton == null) return;

        bool isActive = buttonsSettingsPanel.activeSelf;
        buttonsSettingsPanel.SetActive(!isActive);

        var img = buttonsHeaderButton.GetComponent<Image>();
        if (img != null)
            img.sprite = isActive ? buttonsSpriteClosed : buttonsSpriteOpened;
    }

    // Auto-save for inputs
    void HookAutoSave(SliderGroup group)
    {
        if (group.minValueField)
            group.minValueField.onEndEdit.AddListener(_ => SaveSliderGroup(group));

        if (group.maxValueField)
            group.maxValueField.onEndEdit.AddListener(_ => SaveSliderGroup(group));

        if (group.startValueField)
            group.startValueField.onEndEdit.AddListener(_ => SaveSliderGroup(group));
    }

    // Radio using LeanToggle (two options: 0 = Off, 1 = On)
    void HookDirectionRadios(SliderGroup group)
    {
        if (group.directionOptions == null || group.directionOptions.Count < 2) return;

        for (int i = 0; i < group.directionOptions.Count; i++)
        {
            int idx = i;
            var t = group.directionOptions[idx];
            if (t == null) continue;

            // We'll control siblings ourselves
            t.TurnOffSiblings = false;

            // When an option turns ON, switch selection
            t.OnOn.AddListener(() => OnDirectionSelected(group, idx));

            // When the active option turns OFF by user, snap it back ON to keep radio valid
            t.OnOff.AddListener(() =>
            {
                if (!isProgrammatic && group.currentDirectionIndex == idx)
                {
                    t.On = true;
                }
            });
        }

        // Ensure UI matches loaded state
        ApplyDirectionUI(group, group.currentDirectionIndex, playTransitions: false);
    }

    void OnDirectionSelected(SliderGroup group, int newIndex)
    {
        if (newIndex == group.currentDirectionIndex) return;

        ApplyDirectionUI(group, newIndex, playTransitions: true);
        SaveSliderGroup(group); // persists and updates robotArmSelection
    }

    void ApplyDirectionUI(SliderGroup group, int index, bool playTransitions)
    {
        if (group.directionOptions == null || group.directionOptions.Count < 2) return;

        isProgrammatic = true;

        for (int i = 0; i < group.directionOptions.Count; i++)
        {
            var t = group.directionOptions[i];
            if (t == null) continue;

            bool shouldBeOn = (i == index);
            if (playTransitions)
            {
                if (shouldBeOn) t.TurnOn(); else t.TurnOff();
            }
            else
            {
                t.On = shouldBeOn;
            }
        }

        group.currentDirectionIndex = index; // 0 = Off, 1 = On

        isProgrammatic = false;
    }

    void SaveSliderGroup(SliderGroup group)
    {
        int min = ParseInt(group.minValueField, 0);
        int max = ParseInt(group.maxValueField, 180);
        int start = ParseInt(group.startValueField, 90);

        min = Mathf.Max(min, 0);
        max = Mathf.Max(max, min);
        start = Mathf.Clamp(start, min, max);

        // Map radio to flip: index 0 = Off (flip=0), index 1 = On (flip=1)
        int flipInt = Mathf.Clamp(group.currentDirectionIndex, 0, 1);
        bool flip = flipInt == 1;

        // Save
        if (!string.IsNullOrEmpty(group.minKey)) PlayerPrefs.SetInt(group.minKey, min);
        if (!string.IsNullOrEmpty(group.maxKey)) PlayerPrefs.SetInt(group.maxKey, max);
        if (!string.IsNullOrEmpty(group.startKey)) PlayerPrefs.SetInt(group.startKey, start);
        if (!string.IsNullOrEmpty(group.flipKey)) PlayerPrefs.SetInt(group.flipKey, flip ? 1 : 0);
        PlayerPrefs.Save();

        // Apply to robot
        int index = sliderGroups.IndexOf(group);
        if (index >= 0)
            robotArmSelection?.ConfigureSliderValue(index, min, max, start, flip);
    }

    void LoadSliderGroup(SliderGroup group)
    {
        int min = PlayerPrefs.GetInt(group.minKey, 0);
        int max = PlayerPrefs.GetInt(group.maxKey, 180);
        int start = PlayerPrefs.GetInt(group.startKey, 90);
        int flipI = PlayerPrefs.GetInt(group.flipKey, 0); // 0 = Off, 1 = On

        if (group.minValueField) group.minValueField.text = min.ToString();
        if (group.maxValueField) group.maxValueField.text = max.ToString();
        if (group.startValueField) group.startValueField.text = start.ToString();

        // Update radio state to match saved flip
        group.currentDirectionIndex = Mathf.Clamp(flipI, 0, 1);
        ApplyDirectionUI(group, group.currentDirectionIndex, playTransitions: false);

        // Push to robot
        int index = sliderGroups.IndexOf(group);
        if (index >= 0)
            robotArmSelection?.ConfigureSliderValue(index, min, max, start, flipI == 1);
    }

    void SaveButtonValues()
    {
        int open = ParseInt(openButtonValueIn, 105);
        int close = ParseInt(closeButtonValueIn, 177);

        PlayerPrefs.SetInt(openKey, open);
        PlayerPrefs.SetInt(closeKey, close);
        PlayerPrefs.Save();

        robotArmSelection?.ConfigureOpenCloseValues(open, close);
    }

    int ParseInt(TMP_InputField field, int defaultValue)
    {
        if (field == null) return defaultValue;
        return int.TryParse(field.text, out int v) ? v : defaultValue;
    }

    // Show/hide this panel and update visible slider main buttons based on SelectedModelIndex
    public void ShowSliderConfigPanel()
    {
        if (sliderConfigPanel != null) sliderConfigPanel.SetActive(true);

        // Update which main buttons are visible each time the panel is shown
        UpdateVisibleMainButtonsByModel();
    }

    public void HideSliderConfigPanel()
    {
        if (sliderConfigPanel != null) sliderConfigPanel.SetActive(false);
    }

    // Determine and apply visibility of slider main buttons according to selected model
    void UpdateVisibleMainButtonsByModel()
    {
        int selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        // Map:
        // 0 -> show sliders 1..3
        // 1 or 2 (5 / 5b) -> show sliders 1..4
        // 3 (6) -> show sliders 1..5
        int visibleCount;
        if (selectedModelIndex == 0)
        {
            visibleCount = 3;
        }
        else if (selectedModelIndex == 1 || selectedModelIndex == 2)
        {
            visibleCount = 4;
        }
        else if (selectedModelIndex == 3)
        {
            visibleCount = 5;
        }
        else
        {
            // Fallback: clamp to available groups, defaulting to 3 minimum if possible
            visibleCount = Mathf.Clamp(3, 0, sliderGroups.Count);
        }

        for (int i = 0; i < sliderGroups.Count; i++)
        {
            var group = sliderGroups[i];
            bool shouldShow = i < visibleCount;

            if (group.mainButton != null)
                group.mainButton.gameObject.SetActive(shouldShow);

            // If hiding a group, also collapse its settings panel and set closed sprite
            if (!shouldShow)
            {
                if (group.settingsPanel != null)
                    group.settingsPanel.SetActive(false);

                if (group.mainButton != null)
                {
                    var img = group.mainButton.GetComponent<Image>();
                    if (img != null && group.spriteClosed != null)
                        img.sprite = group.spriteClosed;
                }
            }
        }
    }
}