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
    public Transform expandIconTransform;

    [Header("Fields")]
    public TMP_InputField minValueField;
    public TMP_InputField maxValueField;
    public TMP_InputField startValueField;

    [Header("Direction radio (index 0 = Off, index 1 = On)")]
    public List<LeanToggle> directionOptions;

    [HideInInspector] public string minKey;
    [HideInInspector] public string maxKey;
    [HideInInspector] public string startKey;
    [HideInInspector] public string flipKey;

    [HideInInspector] public int currentDirectionIndex;
    [HideInInspector] public bool isAccordionOpen; // runtime only
}

public class SliderValueConfig : MonoBehaviour
{
    public GameObject sliderConfigPanel;

    [Header("Core")]
    public RobotArmSelection robotArmSelection;
    public SliderTextUpdater sliderTextUpdater;

    public List<SliderGroup> sliderGroups = new List<SliderGroup>();

    [Header("Open/Close values (fields)")]
    public TMP_InputField openButtonValueIn;
    public TMP_InputField closeButtonValueIn;

    [Header("Buttons group (accordion)")]
    public Button buttonsHeaderButton;
    public GameObject buttonsSettingsPanel;
    public Sprite buttonsSpriteClosed;
    public Sprite buttonsSpriteOpened;
    public Transform buttonsExpandIcon;

    // --- NEW: Continuous send controls ---
    [Header("Send Continuously Controls")]
    public LeanToggle sendContinuouslyToggle;
    public Button sendSettingsButton;
    public GameObject sendSettingsPanel;
    public TMP_InputField sendIntervalInput;

    // Keys for PlayerPrefs (only for values, not accordion states)
    [HideInInspector] public string openKey;
    [HideInInspector] public string closeKey;
    private const string SendContinuouslyKey = "SendContinuously";
    private const string SendIntervalStepKey = "SendIntervalStep";

    private bool isProgrammatic;
    private int selectedModel;

    // runtime-only state for buttons accordion
    private bool isButtonsAccordionOpen;

    void Awake()
    {
        for (int i = 0; i < sliderGroups.Count; i++)
        {
            int n = i + 1;
            sliderGroups[i].minKey = $"Slider{n}_Min";
            sliderGroups[i].maxKey = $"Slider{n}_Max";
            sliderGroups[i].startKey = $"Slider{n}_Start";
            sliderGroups[i].flipKey = $"Slider{n}_FlipDirection";
            sliderGroups[i].isAccordionOpen = false; // default collapsed
        }

        openKey = "OpenButtonValue";
        closeKey = "CloseButtonValue";
    }

    void Start()
    {
        foreach (var group in sliderGroups)
        {
            if (group.settingsPanel != null)
                group.settingsPanel.SetActive(group.isAccordionOpen);

            if (group.mainButton != null)
            {
                var img = group.mainButton.GetComponent<Image>();
                if (img != null && group.spriteClosed != null)
                    img.sprite = group.isAccordionOpen ? group.spriteOpened : group.spriteClosed;

                if (group.expandIconTransform != null)
                {
                    group.expandIconTransform.eulerAngles = new Vector3(0, 0, group.isAccordionOpen ? 180f : 0f);
                }
                
                group.mainButton.onClick.AddListener(() => ToggleGroup(group));
            }

            LoadSliderGroup(group);
            HookAutoSave(group);
            HookDirectionRadios(group);
        }

        if (buttonsSettingsPanel != null)
            buttonsSettingsPanel.SetActive(isButtonsAccordionOpen);

        if (buttonsHeaderButton != null)
        {
            var img = buttonsHeaderButton.GetComponent<Image>();
            if (img != null && buttonsSpriteClosed != null)
                img.sprite = isButtonsAccordionOpen ? buttonsSpriteOpened : buttonsSpriteClosed;

            if (buttonsExpandIcon != null)
            {
                buttonsExpandIcon.eulerAngles = new Vector3(0, 0, isButtonsAccordionOpen ? 180f : 0f);
            }

            buttonsHeaderButton.onClick.AddListener(ToggleButtonsAccordion);
        }

        // USING REGISTRY FOR DEFAULTS
        int open = PlayerPrefsKeyRegistry.GetInt(openKey);
        int close = PlayerPrefsKeyRegistry.GetInt(closeKey);
        robotArmSelection?.ConfigureOpenCloseValues(open, close);

        if (openButtonValueIn) openButtonValueIn.text = open.ToString();
        if (closeButtonValueIn) closeButtonValueIn.text = close.ToString();

        if (openButtonValueIn) openButtonValueIn.onEndEdit.AddListener(_ => SaveButtonValues());
        if (closeButtonValueIn) closeButtonValueIn.onEndEdit.AddListener(_ => SaveButtonValues());

        // USING REGISTRY FOR DEFAULTS
        bool sendContinuously = PlayerPrefsKeyRegistry.GetInt(SendContinuouslyKey) == 1;
        int sendStep = PlayerPrefsKeyRegistry.GetInt(SendIntervalStepKey);

        if (sendContinuouslyToggle != null)
        {
            sendContinuouslyToggle.On = sendContinuously;
            sendContinuouslyToggle.OnOn.AddListener(() => OnSendToggleChanged(true));
            sendContinuouslyToggle.OnOff.AddListener(() => OnSendToggleChanged(false));
        }

        if (sendIntervalInput != null)
            sendIntervalInput.text = sendStep.ToString();

        if (sendSettingsButton != null)
            sendSettingsButton.onClick.AddListener(OnSendSettingsButtonClicked);

        if (sendSettingsPanel != null)
            sendSettingsPanel.SetActive(false);

        robotArmSelection?.SetSendContinuouslyMode(sendContinuously, sendStep);
        sliderTextUpdater.RefreshAllFromPrefs();
    }

    public void ShowSliderConfigPanel()
    {
        sliderConfigPanel.SetActive(true);
        selectedModel = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        // Determine how many slider groups to show based on selected model
        int visibleCount = selectedModel switch
        {
            0 => 3,
            1 => 4,
            2 => 4,
            3 => 5,
            _ => 0
        };

        for (int i = 0; i < sliderGroups.Count; i++)
        {
            bool shouldShow = i < visibleCount;
            if (sliderGroups[i].mainButton != null)
                sliderGroups[i].mainButton.gameObject.SetActive(shouldShow);

            if (sliderGroups[i].settingsPanel != null)
                sliderGroups[i].settingsPanel.SetActive(sliderGroups[i].isAccordionOpen && shouldShow);
        }

        if (buttonsSettingsPanel != null)
            buttonsSettingsPanel.SetActive(isButtonsAccordionOpen);

        PanelManager.Instance.PushPanel(
            key: sliderConfigPanel,
            hide: () => HideSliderConfigPanel(), 
            isActive: () => sliderConfigPanel != null && sliderConfigPanel.activeSelf
        );
        

    }

    public void HideSliderConfigPanel()
    {
        sliderConfigPanel.SetActive(false);
        sliderTextUpdater.RefreshAllFromPrefs();
    }

    void ToggleGroup(SliderGroup group)
    {
        if (group.settingsPanel == null || group.mainButton == null) return;

        group.isAccordionOpen = !group.isAccordionOpen;
        group.settingsPanel.SetActive(group.isAccordionOpen);

        var img = group.mainButton.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = group.isAccordionOpen ? group.spriteOpened : group.spriteClosed;
        }

        if (group.expandIconTransform != null)
        {
            float targetZ = group.isAccordionOpen ? 180f : 0f;
            group.expandIconTransform.eulerAngles = new Vector3(0, 0, targetZ);
        }
    }

    void ToggleButtonsAccordion()
    {
        if (buttonsSettingsPanel == null || buttonsHeaderButton == null) return;

        isButtonsAccordionOpen = !isButtonsAccordionOpen;
        buttonsSettingsPanel.SetActive(isButtonsAccordionOpen);

        var img = buttonsHeaderButton.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = isButtonsAccordionOpen ? buttonsSpriteOpened : buttonsSpriteClosed;
        }

        if (buttonsExpandIcon != null)
        {
            float targetZ = isButtonsAccordionOpen ? 180f : 0f;
            buttonsExpandIcon.eulerAngles = new Vector3(0, 0, targetZ);
        }
    }

    void LoadSliderGroup(SliderGroup group)
    {
        isProgrammatic = true;

        // USING REGISTRY FOR DEFAULTS
        int min = PlayerPrefsKeyRegistry.GetInt(group.minKey);
        int max = PlayerPrefsKeyRegistry.GetInt(group.maxKey);
        int start = PlayerPrefsKeyRegistry.GetInt(group.startKey);

        if (group.minValueField != null)
            group.minValueField.text = min.ToString();

        if (group.maxValueField != null)
            group.maxValueField.text = max.ToString();

        start = Mathf.Clamp(start, min, max);

        if (group.startValueField != null)
            group.startValueField.text = start.ToString();

        if (group.directionOptions != null && group.directionOptions.Count > 0)
        {
            // USING REGISTRY FOR DEFAULTS
            int dirIndex = PlayerPrefsKeyRegistry.GetInt(group.flipKey);
            group.currentDirectionIndex = dirIndex;

            for (int i = 0; i < group.directionOptions.Count; i++)
            {
                group.directionOptions[i].On = (i == dirIndex);
            }
        }

        isProgrammatic = false;
    }

    void HookAutoSave(SliderGroup group)
    {
        if (group.minValueField != null)
            group.minValueField.onEndEdit.AddListener(val => SaveSliderGroup(group));

        if (group.maxValueField != null)
            group.maxValueField.onEndEdit.AddListener(val => SaveSliderGroup(group));

        if (group.startValueField != null)
        {
            group.startValueField.onEndEdit.AddListener(val =>
            {
                ClampAndUpdateStartValue(group);
                SaveSliderGroup(group);
            });
        }
    }

    void HookDirectionRadios(SliderGroup group)
    {
        if (group.directionOptions == null || group.directionOptions.Count == 0)
            return;

        for (int i = 0; i < group.directionOptions.Count; i++)
        {
            int idx = i;
            group.directionOptions[idx].TurnOffSiblings = false;

            group.directionOptions[idx].OnOn.AddListener(() =>
            {
                if (isProgrammatic) return;
                if (group.currentDirectionIndex == idx) return;

                group.currentDirectionIndex = idx;

                isProgrammatic = true;
                for (int j = 0; j < group.directionOptions.Count; j++)
                {
                    if (j != idx)
                        group.directionOptions[j].TurnOff();
                }
                isProgrammatic = false;

                SaveSliderGroup(group);
            });

            group.directionOptions[idx].OnOff.AddListener(() =>
            {
                if (!isProgrammatic && group.currentDirectionIndex == idx)
                {
                    group.directionOptions[idx].On = true;
                }
            });
        }
    }

    void ClampAndUpdateStartValue(SliderGroup group)
    {
        if (group.minValueField == null || group.maxValueField == null || group.startValueField == null)
            return;

        bool parsedMin = int.TryParse(group.minValueField.text, out int min);
        bool parsedMax = int.TryParse(group.maxValueField.text, out int max);
        bool parsedStart = int.TryParse(group.startValueField.text, out int start);

        if (!parsedMin) min = 0;
        if (!parsedMax) max = 180;
        if (!parsedStart) start = min;

        int clampedStart = Mathf.Clamp(start, min, max);

        if (clampedStart != start)
        {
            isProgrammatic = true;
            group.startValueField.text = clampedStart.ToString();
            isProgrammatic = false;
        }
    }

    void SaveSliderGroup(SliderGroup group)
    {
        ClampAndUpdateStartValue(group);

        if (group.minValueField != null && int.TryParse(group.minValueField.text, out int min))
            PlayerPrefs.SetInt(group.minKey, min);

        if (group.maxValueField != null && int.TryParse(group.maxValueField.text, out int max))
            PlayerPrefs.SetInt(group.maxKey, max);

        if (group.startValueField != null && int.TryParse(group.startValueField.text, out int start))
            PlayerPrefs.SetInt(group.startKey, start);

        PlayerPrefs.SetInt(group.flipKey, group.currentDirectionIndex);

        PlayerPrefs.Save();

        int sliderIndex = sliderGroups.IndexOf(group);
        if (sliderIndex >= 0)
        {
            robotArmSelection.ConfigureSliderValue(
                sliderIndex,
                PlayerPrefs.GetInt(group.minKey, 0),
                PlayerPrefs.GetInt(group.maxKey, 180),
                PlayerPrefs.GetInt(group.startKey, 90),
                group.currentDirectionIndex == 1);
        }
    }

    void SaveButtonValues()
    {
        if (int.TryParse(openButtonValueIn.text, out int openVal))
        {
            PlayerPrefs.SetInt(openKey, openVal);
            robotArmSelection.ConfigureOpenCloseValues(openVal, PlayerPrefs.GetInt(closeKey, 177));
        }

        if (int.TryParse(closeButtonValueIn.text, out int closeVal))
        {
            PlayerPrefs.SetInt(closeKey, closeVal);
            robotArmSelection.ConfigureOpenCloseValues(PlayerPrefs.GetInt(openKey, 105), closeVal);
        }

        PlayerPrefs.Save();
    }

    void OnSendToggleChanged(bool isOn)
    {
        int step = 1;
        if (sendIntervalInput != null && int.TryParse(sendIntervalInput.text, out int s))
            step = Mathf.Max(1, s);

        PlayerPrefs.SetInt(SendContinuouslyKey, isOn ? 1 : 0);
        PlayerPrefs.SetInt(SendIntervalStepKey, step);
        PlayerPrefs.Save();

        robotArmSelection.SetSendContinuouslyMode(isOn, step);

        if (!isOn && sendSettingsPanel != null)
            sendSettingsPanel.SetActive(false);

        UpdateSendSettingsButtonSprite();
    }

    void OnSendSettingsButtonClicked()
    {
        if (sendSettingsPanel == null) return;

        bool isActive = sendSettingsPanel.activeSelf;

        if (sendContinuouslyToggle != null && !sendContinuouslyToggle.On)
            return;

        sendSettingsPanel.SetActive(!isActive);

        UpdateSendSettingsButtonSprite();
    }

    void UpdateSendSettingsButtonSprite()
    {
        if (sendSettingsButton == null) return;

        var img = sendSettingsButton.GetComponent<Image>();
        if (img == null) return;

        bool isOpen = sendSettingsPanel != null && sendSettingsPanel.activeSelf;
        img.sprite = isOpen ? buttonsSpriteOpened : buttonsSpriteClosed;
    }
}