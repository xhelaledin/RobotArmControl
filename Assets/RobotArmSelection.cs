using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lean.Gui;

public class RobotArmSelection : MonoBehaviour
{
    public BluetoothCommandConstructor bluetoothCommandConstructor;

    public GameObject armModel4;
    public GameObject armModel5;
    public GameObject armModel5b;
    public GameObject armModel6;

    public RobotArmInputHandler4Parts robotArmInputHandler4Parts; // 4-part arm
    public RobotArmInputHandler5Parts robotArmInputHandler5Parts; // 5-part arm
    public RobotArmInputHandler5BParts robotArmInputHandler5BParts; // 5b-part arm 
    public RobotArmInputHandler6Parts robotArmInputHandler6Parts; // 6-part arm

    public int selectedModelIndex;

    public Slider slider1, slider2, slider3, slider4, slider5;

    private Slider[] _sliders;

    // These now default to values from the Registry in Awake
    public int openValue;
    public int closeValue;

    [Header("Continuous Send Settings")]
    public LeanToggle sendContinuouslyToggle;
    public Button settingsButton;
    public GameObject settingsPanel;
    public Sprite buttonsSpriteClosed;
    public Sprite buttonsSpriteOpened;
    public TMP_InputField sendIntervalInput;

    private bool sendContinuously = false;
    private int sendIntervalStep = 1;

    private Coroutine sendCoroutine;

    // Track last sent stepped value for each slider
    private int[] lastSentSteppedValues = new int[5];

    // Track if slider was touched by user for continuous send
    private bool[] sliderTouched = new bool[5];

    private bool openClawOnStart = false;

    private void Awake()
    {
        // Use Registry to get the selected model (Default is 0)
        selectedModelIndex = PlayerPrefsKeyRegistry.GetInt("SelectedModelIndex");

        // Load Open/Close button defaults from Registry
        openValue = PlayerPrefsKeyRegistry.GetInt("OpenButtonValue");
        closeValue = PlayerPrefsKeyRegistry.GetInt("CloseButtonValue");

        _sliders = new[]
        {
            slider1,
            slider2,
            slider3,
            slider4,
            slider5
        };

        for (int i = 0; i < lastSentSteppedValues.Length; i++)
            lastSentSteppedValues[i] = int.MinValue;

        ResetSliderTouchedFlags();

        // Use Registry for Continuous Send settings
        sendContinuously = PlayerPrefsKeyRegistry.GetInt("SendContinuously") == 1;
        sendIntervalStep = PlayerPrefsKeyRegistry.GetInt("SendIntervalStep");

        if (sendIntervalInput != null)
            sendIntervalInput.text = sendIntervalStep.ToString();

        if (sendContinuouslyToggle != null)
        {
            sendContinuouslyToggle.On = sendContinuously;
            sendContinuouslyToggle.OnOn.AddListener(() => SetSendContinuouslyMode(true, GetSendIntervalStep()));
            sendContinuouslyToggle.OnOff.AddListener(() => SetSendContinuouslyMode(false, GetSendIntervalStep()));
        }

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        UpdateSettingsButtonSprite();
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        OnModelSelected(selectedModelIndex);

        // Subscribe to slider value changes and mark touched on user input
        slider1.onValueChanged.AddListener(value => OnSliderValueChanged(0, value, 1));
        slider2.onValueChanged.AddListener(value => OnSliderValueChanged(1, value, 1));
        slider3.onValueChanged.AddListener(value => OnSliderValueChanged(2, value, 1));
        slider4.onValueChanged.AddListener(value => OnSliderValueChanged(3, value, 1));
        slider5.onValueChanged.AddListener(value => OnSliderValueChanged(4, value, 1));

        slider1.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(() => OnSliderReleased(0)));
        slider2.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(() => OnSliderReleased(1)));
        slider3.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(() => OnSliderReleased(2)));
        slider4.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(() => OnSliderReleased(3)));
        slider5.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(() => OnSliderReleased(4)));

        // *** Load saved slider configs from Registry and apply ***
        LoadAndApplySavedSliderConfigs();

        // Then move model parts to saved start positions
        MoveModelByStartValues();

        if (sendContinuously)
            StartSendCoroutine();
    }

    private void LoadAndApplySavedSliderConfigs()
    {
        for (int i = 0; i < _sliders.Length; i++)
        {
            // UPDATED: Keys must match the Registry exactly
            // Registry uses "SliderX_FlipDirection", not "SliderX_Flip"
            string minKey = $"Slider{i + 1}_Min";
            string maxKey = $"Slider{i + 1}_Max";
            string startKey = $"Slider{i + 1}_Start";
            string flipKey = $"Slider{i + 1}_FlipDirection"; 

            // Use PlayerPrefsKeyRegistry to fetch values with correct defaults
            int min = PlayerPrefsKeyRegistry.GetInt(minKey);
            int max = PlayerPrefsKeyRegistry.GetInt(maxKey);
            int start = PlayerPrefsKeyRegistry.GetInt(startKey);
            bool flip = PlayerPrefsKeyRegistry.GetInt(flipKey) == 1;

            ConfigureSliderValue(i, min, max, start, flip);
        }
    }

    private int GetSendIntervalStep()
    {
        if (sendIntervalInput == null) return 1;
        if (int.TryParse(sendIntervalInput.text, out int val))
            return Mathf.Max(1, val);
        return 1;
    }

    private void OnSettingsButtonClicked()
    {
        if (!sendContinuously)
            return; // Only allow if continuous send is ON

        if (settingsPanel == null) return;

        bool isActive = settingsPanel.activeSelf;
        settingsPanel.SetActive(!isActive);

        UpdateSettingsButtonSprite();
    }

    private void UpdateSettingsButtonSprite()
    {
        if (settingsButton == null) return;
        var img = settingsButton.GetComponent<Image>();
        if (img == null) return;

        bool isOpen = settingsPanel != null && settingsPanel.activeSelf;
        img.sprite = isOpen ? buttonsSpriteOpened : buttonsSpriteClosed;
    }

    public void SetSendContinuouslyMode(bool on, int step)
    {
        sendContinuously = on;
        sendIntervalStep = Mathf.Max(1, step);

        // Standard PlayerPrefs Set remains (Registry doesn't handle setting)
        PlayerPrefs.SetInt("SendContinuously", on ? 1 : 0);
        PlayerPrefs.SetInt("SendIntervalStep", sendIntervalStep);
        PlayerPrefs.Save();

        if (settingsPanel != null && !on)
        {
            settingsPanel.SetActive(false);
            UpdateSettingsButtonSprite();
        }

        if (sendCoroutine != null)
        {
            StopCoroutine(sendCoroutine);
            sendCoroutine = null;
        }

        for (int i = 0; i < lastSentSteppedValues.Length; i++)
            lastSentSteppedValues[i] = int.MinValue;

        ResetSliderTouchedFlags();

        if (on)
        {
            StartSendCoroutine();
        }
    }

    private void ResetSliderTouchedFlags()
    {
        for (int i = 0; i < sliderTouched.Length; i++)
            sliderTouched[i] = false;
    }

    private void StartSendCoroutine()
    {
        sendCoroutine = StartCoroutine(ContinuousSendRoutine());
    }

    private IEnumerator ContinuousSendRoutine()
    {
        while (sendContinuously)
        {
            for (int i = 0; i < _sliders.Length; i++)
            {
                if (!_sliders[i].gameObject.activeSelf)
                    continue;

                if (!sliderTouched[i])
                    continue; // Only send for sliders touched by user

                float val = _sliders[i].value;

                int minVal = Mathf.RoundToInt(_sliders[i].minValue);
                int maxVal = Mathf.RoundToInt(_sliders[i].maxValue);

                val = Mathf.Clamp(val, minVal, maxVal);

                int steppedVal = CalculateSteppedValue(val, minVal, maxVal, sendIntervalStep);

                if (steppedVal != lastSentSteppedValues[i])
                {
                    lastSentSteppedValues[i] = steppedVal;
                    SendSliderCommand(i, steppedVal);
                    ApplyRotation(i, steppedVal, 1);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private int CalculateSteppedValue(float value, int min, int max, int step)
    {
        if (step <= 1) return Mathf.RoundToInt(value);

        int offset = Mathf.RoundToInt(value) - min;
        int steppedOffset = Mathf.RoundToInt(Mathf.Round((float)offset / step) * step);
        int steppedValue = min + steppedOffset;
        return Mathf.Clamp(steppedValue, min, max);
    }

    private void OnSliderValueChanged(int sliderIndex, float value, int outlineIndex)
    {
        // Mark slider as touched by user
        sliderTouched[sliderIndex] = true;

        // Apply visual rotation immediately
        ApplyRotation(sliderIndex, value, outlineIndex);

        // No sending here; continuous send coroutine or release handles sending
    }

    private void OnSliderReleased(int sliderIndex)
    {
        float val = _sliders[sliderIndex].value;
        SendSliderCommand(sliderIndex, val);

        int minVal = Mathf.RoundToInt(_sliders[sliderIndex].minValue);
        int maxVal = Mathf.RoundToInt(_sliders[sliderIndex].maxValue);
        int steppedVal = CalculateSteppedValue(val, minVal, maxVal, sendIntervalStep);

        lastSentSteppedValues[sliderIndex] = steppedVal;

        // Mark touched so continuous send keeps working after release
        sliderTouched[sliderIndex] = true;
    }

    private void SendSliderCommand(int sliderIndex, float value)
    {
        int roundedValue = Mathf.RoundToInt(value);

        switch (sliderIndex)
        {
            case 0:
                bluetoothCommandConstructor.ConstructSlider1Command(roundedValue.ToString());
                break;
            case 1:
                bluetoothCommandConstructor.ConstructSlider2Command(roundedValue.ToString());
                break;
            case 2:
                bluetoothCommandConstructor.ConstructSlider3Command(roundedValue.ToString());
                break;
            case 3:
                bluetoothCommandConstructor.ConstructSlider4Command(roundedValue.ToString());
                break;
            case 4:
                bluetoothCommandConstructor.ConstructSlider5Command(roundedValue.ToString());
                break;
        }
    }

    private void ApplyRotation(int sliderIndex, float value, int outlineIndex)
    {
        switch (sliderIndex)
        {
            case 0:
                switch (selectedModelIndex)
                {
                    case 0: robotArmInputHandler4Parts.setPart1Rotation(value, outlineIndex); break;
                    case 1: robotArmInputHandler5Parts.setPart1Rotation(value, outlineIndex); break;
                    case 2: robotArmInputHandler5BParts.setPart1Rotation(value, outlineIndex); break;
                    case 3: robotArmInputHandler6Parts.setPart1Rotation(value, outlineIndex); break;
                }
                break;
            case 1:
                switch (selectedModelIndex)
                {
                    case 0: robotArmInputHandler4Parts.setPart2Rotation(value, outlineIndex); break;
                    case 1: robotArmInputHandler5Parts.setPart2Rotation(value, outlineIndex); break;
                    case 2: robotArmInputHandler5BParts.setPart2Rotation(value, outlineIndex); break;
                    case 3: robotArmInputHandler6Parts.setPart2Rotation(value, outlineIndex); break;
                }
                break;
            case 2:
                switch (selectedModelIndex)
                {
                    case 0: robotArmInputHandler4Parts.setPart3Rotation(value, outlineIndex); break;
                    case 1: robotArmInputHandler5Parts.setPart3Rotation(value, outlineIndex); break;
                    case 2: robotArmInputHandler5BParts.setPart3Rotation(value, outlineIndex); break;
                    case 3: robotArmInputHandler6Parts.setPart3Rotation(value, outlineIndex); break;
                }
                break;
            case 3:
                switch (selectedModelIndex)
                {
                    case 1: robotArmInputHandler5Parts.setPart4Rotation(value, outlineIndex); break;
                    case 2: robotArmInputHandler5BParts.setPart4Rotation(value, outlineIndex); break;
                    case 3: robotArmInputHandler6Parts.setPart4Rotation(value, outlineIndex); break;
                }
                break;
            case 4:
                if (selectedModelIndex == 3)
                {
                    robotArmInputHandler6Parts.setPart5Rotation(value, outlineIndex);
                }
                break;
        }
    }

    public void UpdateSelectedModelIndex()
    {
        // Use Registry to get current index
        selectedModelIndex = PlayerPrefsKeyRegistry.GetInt("SelectedModelIndex");
    }

    public void ConfigureSliderValue(int index, int min, int max, int start, bool flipDirection)
    {
        if (index < 0 || index >= _sliders.Length)
        {
            Debug.LogWarning($"Slider index {index} is out of range (0–4).");
            return;
        }

        var s = _sliders[index];
        s.minValue = min;
        s.maxValue = max;
        s.value = Mathf.Clamp(start, min, max);

        Slider.Direction direction = flipDirection ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;
        s.SetDirection(direction, true);

        // Reset last sent stepped value for this slider to force sending on next continuous send
        lastSentSteppedValues[index] = int.MinValue;

        // Reset touched flag so no send happens until user moves slider
        sliderTouched[index] = false;
    }

    public void ConfigureOpenCloseValues(int openValue, int closeValue)
    {
        this.openValue = openValue;
        this.closeValue = closeValue;
    }

    private EventTrigger.Entry CreatePointerUpTrigger(UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entry.callback.AddListener((data) => action.Invoke());
        return entry;
    }

    public void OnModelSelected(int index)
    {
        selectedModelIndex = index;

        armModel4.SetActive(index == 0);
        armModel5.SetActive(index == 1);
        armModel5b.SetActive(index == 2);
        armModel6.SetActive(index == 3);

        slider1.gameObject.SetActive(true);
        slider2.gameObject.SetActive(true);
        slider3.gameObject.SetActive(true);

        slider4.gameObject.SetActive(index == 1 || index == 2 || index == 3);
        slider5.gameObject.SetActive(index == 3);
    }

    public void OpenClawOnStart()
    {
        openClawOnStart = true;
        OnOpenButtonPressed();
        openClawOnStart = false;
    }

    public void OnOpenButtonPressed()
    {
        bluetoothCommandConstructor.ConstructOpenCommand(openValue.ToString());

        int outlineValue = openClawOnStart ? 0 : 1;
        switch (selectedModelIndex)
        {
            case 0: robotArmInputHandler4Parts.OpenClaw(outlineValue); break;
            case 1: robotArmInputHandler5Parts.OpenClaw(outlineValue); break;
            case 2: robotArmInputHandler5BParts.OpenClaw(outlineValue); break;
            case 3: robotArmInputHandler6Parts.OpenClaw(outlineValue); break;
        }

        PlayerPrefs.SetInt("OpenButtonPressed", 1);
        PlayerPrefs.SetInt("CloseButtonPressed", 0);
        PlayerPrefs.Save();
    }

    public void OnCloseButtonPressed()
    {
        bluetoothCommandConstructor.ConstructCloseCommand(closeValue.ToString());

        switch (selectedModelIndex)
        {
            case 0: robotArmInputHandler4Parts.CloseClaw(1); break;
            case 1: robotArmInputHandler5Parts.CloseClaw(1); break;
            case 2: robotArmInputHandler5BParts.CloseClaw(1); break;
            case 3: robotArmInputHandler6Parts.CloseClaw(1); break;
        }

        PlayerPrefs.SetInt("OpenButtonPressed", 0);
        PlayerPrefs.SetInt("CloseButtonPressed", 1);
        PlayerPrefs.Save();
    }

    public void MoveModelByStartValues()
    {
        // Use Registry to get explicit start values
        int slider1Start = PlayerPrefsKeyRegistry.GetInt("Slider1_Start");
        int slider2Start = PlayerPrefsKeyRegistry.GetInt("Slider2_Start");
        int slider3Start = PlayerPrefsKeyRegistry.GetInt("Slider3_Start");
        int slider4Start = PlayerPrefsKeyRegistry.GetInt("Slider4_Start");
        int slider5Start = PlayerPrefsKeyRegistry.GetInt("Slider5_Start");

        // Set slider values directly, this moves the slider correctly
        slider1.value = slider1Start;
        slider2.value = slider2Start;
        slider3.value = slider3Start;
        slider4.value = slider4Start;
        slider5.value = slider5Start;

        // Then apply rotation visuals
        OnSliderValueChanged(0, slider1Start, 0);
        OnSliderValueChanged(1, slider2Start, 0);
        OnSliderValueChanged(2, slider3Start, 0);
        OnSliderValueChanged(3, slider4Start, 0);
        OnSliderValueChanged(4, slider5Start, 0);

        OpenClawOnStart();

        // *** NEW: Sync buttons. Since we reset to start, no saved "Move" is currently active.
        ResetAllSaveItemButtons();
    }

    // Helper to find all SaveItemManagers in the scene and uncheck their buttons
    private void ResetAllSaveItemButtons()
    {
        // FindObjectsByType works in newer Unity versions. 
        // If you are on an older version, use FindObjectsOfType<SaveItemManager>()
        SaveItemManager[] items = FindObjectsByType<SaveItemManager>(FindObjectsSortMode.None);
        
        foreach (var item in items)
        {
            if (item != null)
            {
                item.SetRunButtonNormal();
                item.SetViewButtonNormal();
            }
        }
    }

    public void MoveModelAfterStartPosEdit()
    {
        OnSliderValueChanged(0, slider1.value, 0);
        OnSliderValueChanged(1, slider2.value, 0);
        OnSliderValueChanged(2, slider3.value, 0);
        OnSliderValueChanged(3, slider4.value, 0);
        OnSliderValueChanged(4, slider5.value, 0);
    }
}