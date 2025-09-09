using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;

public class RobotArmPartPositionController : MonoBehaviour, IHideablePanel
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

    private void Awake()
    {
        sliders = new[] { slider1, slider2, slider3, slider4, slider5 };
        toggles = new[] { toggle1, toggle2, toggle3, toggle4, toggle5 };

        EnsureDefaultPrefs();

        // Wire up sliders & toggles
        for (int i = 0; i < sliders.Length; i++)
        {
            int idx = i;
            sliders[i].onValueChanged.AddListener(v => OnSliderChanged(idx, v));
            toggles[i].onValueChanged.AddListener(b => OnToggleChanged(idx, b));
        }

        // Global mode toggles
        minToggle.TurnOffSiblings = false;
        startToggle.TurnOffSiblings = false;
        maxToggle.TurnOffSiblings = false;

        minToggle.OnOn.AddListener(() => SetMode(Mode.Min));
        startToggle.OnOn.AddListener(() => SetMode(Mode.Start));
        maxToggle.OnOn.AddListener(() => SetMode(Mode.Max));

        SetMode(Mode.Min, playTransitions: false);
        startToggle.On = true;

        RefreshModelSettings();
        LoadPreferences();
    }

    public void ShowPanel()
    {
        RefreshModelSettings();
        ConfigureUI();
        LoadPreferences();
        visualPanel.SetActive(true);

        sliderTextUpdater.LoadStartValuesFromPrefs();
        sliderTextUpdater.UpdateStartCurrentValueText(selectedModelIndex);
        PanelManager.Instance.RegisterPanel(this);
    }

    public void HidePanel()
    {
        visualPanel.SetActive(false);
        modelSelectorRadio.ShowSettingsPanel();
        robotArmSelection.MoveModelByStartValues();
    }

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
            sliders[i].gameObject.SetActive(isActive);
            toggles[i].gameObject.SetActive(isActive);
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

            sliders[i].SetValueWithoutNotify(sliderOffset); // initialize slider
            toggles[i].SetIsOnWithoutNotify(direction);

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

    private void SetMode(Mode mode, bool playTransitions = true)
    {
        currentMode = mode;

        if (playTransitions)
        {
            if (mode == Mode.Min) { startToggle.TurnOff(); maxToggle.TurnOff(); }
            if (mode == Mode.Start) { minToggle.TurnOff(); maxToggle.TurnOff(); }
            if (mode == Mode.Max) { minToggle.TurnOff(); startToggle.TurnOff(); }
        }

        ApplyAllPartsWithMode();
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
                if (index == 0) h4.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h4.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h4.setPart3RotationVisual(finalRotation, outlineIndex);
                break;
            case 1:
                var h5 = robotArmInputHandler5Parts;
                if (index == 0) h5.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h5.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h5.setPart3RotationVisual(finalRotation, outlineIndex);
                if (index == 3) h5.setPart4RotationVisual(finalRotation, outlineIndex);
                break;
            case 2:
                var h5b = robotArmInputHandler5BParts;
                if (index == 0) h5b.setPart1RotationVisual(finalRotation, outlineIndex);
                if (index == 1) h5b.setPart2RotationVisual(finalRotation, outlineIndex);
                if (index == 2) h5b.setPart3RotationVisual(finalRotation, outlineIndex);
                if (index == 3) h5b.setPart4RotationVisual(finalRotation, outlineIndex);
                break;
            case 3:
                var h6 = robotArmInputHandler6Parts;
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
            case 0: robotArmInputHandler4Parts.SetDirection(partIndex, isPositive); break;
            case 1: robotArmInputHandler5Parts.SetDirection(partIndex, isPositive); break;
            case 2: robotArmInputHandler5BParts.SetDirection(partIndex, isPositive); break;
            case 3: robotArmInputHandler6Parts.SetDirection(partIndex, isPositive); break;
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

        robotArmInputHandler4Parts.LoadStartRotationsFromPrefs();
        robotArmInputHandler5Parts.LoadStartRotationsFromPrefs();
        robotArmInputHandler5BParts.LoadStartRotationsFromPrefs();
        robotArmInputHandler6Parts.LoadStartRotationsFromPrefs();

        sliderTextUpdater.ApplyPrefsToSlider(selectedModelIndex);
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
