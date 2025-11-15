using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;

public class RobotArmPartPositionController : MonoBehaviour
{
    [Header("Input Handlers")]
    public RobotArmInputHandler4Parts robotArmInputHandler4Parts;
    public RobotArmInputHandler5Parts robotArmInputHandler5Parts;
    public RobotArmInputHandler5BParts robotArmInputHandler5BParts;
    public RobotArmInputHandler6Parts robotArmInputHandler6Parts;

    [Header("UI Elements")]
    public Slider slider1, slider2, slider3, slider4, slider5;
    public Toggle toggle1, toggle2, toggle3, toggle4, toggle5;
    public GameObject visualPanel;
    public SliderValueConfig sliderValueConfig;

    [Header("Global Radio Toggles (Min/Start/Max)")]
    public LeanToggle minToggle;
    public LeanToggle startToggle;
    public LeanToggle maxToggle;

    private Slider[] sliders;
    private Toggle[] toggles;
    private float[] tempRotations = new float[5];
    private bool[] tempDirections = new bool[5];

    private int selectedModelIndex;
    private int activeParts;
    private string modelName;

    [Header("Classes")]
    public ModelSelectorRadio modelSelectorRadio;
    public RobotArmSelection robotArmSelection;
    public SliderTextUpdater sliderTextUpdater;

    private enum Mode { Min, Start, Max }
    private Mode currentMode = Mode.Min;

    private bool isProgrammatic; // prevents feedback loops

    private void Awake()
    {
        sliders = new[] { slider1, slider2, slider3, slider4, slider5 };
        toggles = new[] { toggle1, toggle2, toggle3, toggle4, toggle5 };

        EnsureDefaultPrefs();

        // Wire up sliders & toggles
        for (int i = 0; i < sliders.Length; i++)
        {
            int idx = i;
            if (sliders[i] != null)
                sliders[i].onValueChanged.AddListener(v => OnSliderChanged(idx, v));
            if (toggles[i] != null)
                toggles[i].onValueChanged.AddListener(b => OnToggleChanged(idx, b));
        }

        // --- Global mode radio toggles ---
        if (minToggle != null) SetupModeRadio(minToggle, Mode.Min);
        if (startToggle != null) SetupModeRadio(startToggle, Mode.Start);
        if (maxToggle != null) SetupModeRadio(maxToggle, Mode.Max);

        // Default to Start mode
        SelectMode(Mode.Start, playTransitions: false);

        RefreshModelSettings();
        LoadPreferences();
    }

    private void SetupModeRadio(LeanToggle toggle, Mode mode)
    {
        toggle.TurnOffSiblings = false;

        // When turned ON → activate mode
        toggle.OnOn.AddListener(() => SelectMode(mode));

        // When turned OFF → if this was the active mode and it wasn’t code-driven, snap back ON
        toggle.OnOff.AddListener(() =>
        {
            if (!isProgrammatic && currentMode == mode)
            {
                toggle.On = true;
            }
        });
    }

    private void SelectMode(Mode mode, bool playTransitions = true)
    {
        if (currentMode == mode && playTransitions)
            return;

        currentMode = mode;
        var activeToggle = GetToggle(mode);
        if (activeToggle == null) return;

        // Turn ON the selected toggle
        if (playTransitions)
            activeToggle.TurnOn();
        else
            activeToggle.On = true;

        // Turn OFF the others
        isProgrammatic = true;
        foreach (var other in new[] { minToggle, startToggle, maxToggle })
        {
            if (other != null && other != activeToggle)
                other.TurnOff();
        }
        isProgrammatic = false;

        ApplyAllPartsWithMode();
    }

    private LeanToggle GetToggle(Mode mode) => mode switch
    {
        Mode.Min => minToggle,
        Mode.Start => startToggle,
        Mode.Max => maxToggle,
        _ => startToggle
    };

    public void ShowPanel()
    {
        RefreshModelSettings();
        ConfigureUI();
        LoadPreferences();
        visualPanel.SetActive(true);

        sliderTextUpdater.LoadStartValuesFromPrefs();
        sliderTextUpdater.UpdateStartCurrentValueText(selectedModelIndex);
        
        PanelManager.Instance.PushPanel(
            key: visualPanel,
            hide: HidePanel,      // Pass the existing HidePanel method
            isActive: IsPanelActive  // Pass the existing IsPanelActive method
        );

    }

    // This method is now called by PanelManager's 'hide' delegate
    public void HidePanel()
    {
        visualPanel.SetActive(false);
        modelSelectorRadio.ShowSettingsPanel();
        robotArmSelection.MoveModelByStartValues();
    }

    // This method is now called by PanelManager's 'isActive' delegate
    public bool IsPanelActive() => visualPanel.activeSelf;

    private void RefreshModelSettings()
    {
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        modelName = GetModelName(selectedModelIndex);
        activeParts = GetActivePartCount(selectedModelIndex);
    }

    private void ConfigureUI()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            bool isActive = i < activeParts;
            if (sliders[i] != null) sliders[i].gameObject.SetActive(isActive);
            if (toggles[i] != null) toggles[i].gameObject.SetActive(isActive);
        }
    }

    public void LoadPreferences()
    {
        RefreshModelSettings();

        for (int i = 0; i < activeParts; i++)
        {
            string offsetKey = $"{modelName}startRotationpart{i + 1}";
            string dirKey = $"{modelName}directionpart{i + 1}";

            float sliderOffset = PlayerPrefs.GetFloat(offsetKey, 0f); // slider offset from PlayerPrefs
            bool direction = PlayerPrefs.GetInt(dirKey, 0) == 1;

            if (sliders[i] != null) sliders[i].SetValueWithoutNotify(sliderOffset); // initialize slider
            if (toggles[i] != null) toggles[i].SetIsOnWithoutNotify(direction);

            tempRotations[i] = sliderOffset;
            tempDirections[i] = direction;

            SetInputHandlerDirection(i, direction);
        }

        ApplyAllPartsWithMode();
    }

    private void OnSliderChanged(int index, float value)
    {
        tempRotations[index] = value;
        ApplyRotation(index, value, 1);
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        tempDirections[index] = isOn;
        SetInputHandlerDirection(index, isOn);
        ApplyRotation(index, sliders[index].value, 1);
    }

    private void ApplyAllPartsWithMode()
    {
        for (int i = 0; i < activeParts; i++)
            ApplyRotation(i, sliders[i].value, 0);
    }

    private void ApplyRotation(int index, float sliderOffset, int outlineIndex)
    {
        string minKey = $"Slider{index + 1}_Min";
        string startKey = $"Slider{index + 1}_Start";
        string maxKey = $"Slider{index + 1}_Max";

        // Baseline for selected mode
        float baseline = currentMode switch
        {
            Mode.Min => PlayerPrefs.GetInt(minKey, 0),
            Mode.Start => PlayerPrefs.GetInt(startKey, 90),
            Mode.Max => PlayerPrefs.GetInt(maxKey, 180),
            _ => 0
        };

        float finalRotation = sliderOffset + baseline;
        // Apply to proper robot arm input handler
        switch (selectedModelIndex)
        {
            case 0:
                var h4 = robotArmInputHandler4Parts;
                if (h4 == null) break;
                if (index == 0) h4.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h4.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h4.setPart3RotationVisual(finalRotation, outlineIndex);
                break;
            case 1:
                var h5 = robotArmInputHandler5Parts;
                if (h5 == null) break;
                if (index == 0) h5.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h5.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h5.setPart3RotationVisual(finalRotation, outlineIndex);
                if (index == 3) h5.setPart4RotationVisual(finalRotation, outlineIndex);
                break;
            case 2:
                var h5b = robotArmInputHandler5BParts;
                if (h5b == null) break;
                if (index == 0) h5b.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h5b.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h5b.setPart3RotationVisual(finalRotation, outlineIndex);
                if (index == 3) h5b.setPart4RotationVisual(finalRotation, outlineIndex);
                break;
            case 3:
                var h6 = robotArmInputHandler6Parts;
                if (h6 == null) break;
                if (index == 0) h6.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h6.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h6.setPart3RotationVisual(finalRotation, outlineIndex);
                if (index == 3) h6.setPart4RotationVisual(finalRotation, outlineIndex);
                if (index == 4) h6.setPart5RotationVisual(finalRotation, outlineIndex);
                break;
        }
    }

    private void SetInputHandlerDirection(int partIndex, bool isPositive)
    {
        switch (selectedModelIndex)
        {
            case 0: robotArmInputHandler4Parts?.SetDirection(partIndex, isPositive); break;
            case 1: robotArmInputHandler5Parts?.SetDirection(partIndex, isPositive); break;
            case 2: robotArmInputHandler5BParts?.SetDirection(partIndex, isPositive); break;
            case 3: robotArmInputHandler6Parts?.SetDirection(partIndex, isPositive); break;
        }
    }

    public void ConfirmChanges()
    {
        RefreshModelSettings();
        for (int i = 0; i < activeParts; i++)
        {
            string offsetKey = $"{modelName}startRotationpart{i + 1}";
            string dirKey = $"{modelName}directionpart{i + 1}";

            PlayerPrefs.SetFloat(offsetKey, tempRotations[i]);
            PlayerPrefs.SetInt(dirKey, tempDirections[i] ? 1 : 0);
            PlayerPrefs.Save();
        }

        HidePanel();
        modelSelectorRadio.ShowSettingsPanel();
        robotArmSelection.MoveModelAfterStartPosEdit();

        robotArmInputHandler4Parts?.LoadStartRotationsFromPrefs();
        robotArmInputHandler5Parts?.LoadStartRotationsFromPrefs();
        robotArmInputHandler5BParts?.LoadStartRotationsFromPrefs();
        robotArmInputHandler6Parts?.LoadStartRotationsFromPrefs();

        sliderTextUpdater?.ApplyPrefsToSlider(selectedModelIndex);
    }

    private string GetModelName(int idx) => idx switch
    {
        0 => "model4",
        1 => "model5",
        2 => "model5B",
        3 => "model6",
        _ => "model4"
    };

    private int GetActivePartCount(int idx) => idx switch
    {
        0 => 3,
        1 => 4,
        2 => 4,
        3 => 5,
        _ => 3
    };

    private void EnsureDefaultPrefs()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (!PlayerPrefs.HasKey($"Slider{i}_Min")) PlayerPrefs.SetInt($"Slider{i}_Min", 0);
            if (!PlayerPrefs.HasKey($"Slider{i}_Start")) PlayerPrefs.SetInt($"Slider{i}_Start", 90);
            if (!PlayerPrefs.HasKey($"Slider{i}_Max")) PlayerPrefs.SetInt($"Slider{i}_Max", 180);
        }
    }
}