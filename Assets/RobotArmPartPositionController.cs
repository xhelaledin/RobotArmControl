using UnityEngine;
using UnityEngine.UI;

public class RobotArmPartPositionController : MonoBehaviour
{
    public RobotArmInputHandler4Parts robotArmInputHandler4Parts;
    public RobotArmInputHandler5Parts robotArmInputHandler5Parts;
    public RobotArmInputHandler5BParts robotArmInputHandler5BParts;
    public RobotArmInputHandler6Parts robotArmInputHandler6Parts;

    public Slider slider1, slider2, slider3, slider4, slider5;
    public Toggle toggle1, toggle2, toggle3, toggle4, toggle5;
    public GameObject visualPanel;
    public SliderValueConfig sliderValueConfig;

    public RobotArmSelection robotArmSelection;

    private Slider[] sliders;
    private Toggle[] toggles;
    private float[] tempRotations = new float[5];
    private bool[]  tempDirections = new bool[5];

    private int selectedModelIndex;
    private int activeParts;
    private string modelName;

    private void Awake()
    {
        sliders = new[] { slider1, slider2, slider3, slider4, slider5 };
        toggles = new[] { toggle1, toggle2, toggle3, toggle4, toggle5 };

        // wire up UI callbacks
        for (int i = 0; i < sliders.Length; i++)
        {
            int idx = i;
            sliders[i].onValueChanged.AddListener(v => OnSliderChanged(idx, v));
            toggles[i].onValueChanged.AddListener(b => OnToggleChanged(idx, b));
        }

        RefreshModelSettings();
        LoadPreferences();
    }

    private void Start()
    {
        // if you need to re-load sliderValueConfig:
        // sliderValueConfig?.LoadSliderValues();
    }

    public void ShowPanel()
    {
        RefreshModelSettings();
        ConfigureUI();
        LoadPreferences();
        visualPanel.SetActive(true);
    }

    public void HidePanel()
    {
        visualPanel.SetActive(false);
    }

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

    /// <summary>
    /// Loads saved base rotations & directions,
    /// updates UI, then applies only the start offsets.
    /// </summary>
    public void LoadPreferences()
    {
        RefreshModelSettings();

        for (int i = 0; i < activeParts; i++)
        {
            string rotKey = $"{modelName}startRotationpart{i + 1}";
            string dirKey = $"{modelName}directionpart{i + 1}";

            float rotation = PlayerPrefs.GetFloat(rotKey, 0f);
            bool  direction = PlayerPrefs.GetInt(dirKey, 0) == 1;

            sliders[i].SetValueWithoutNotify(rotation);
            toggles[i].SetIsOnWithoutNotify(direction);

            tempRotations[i] = rotation;
            tempDirections[i] = direction;

            SetInputHandlerDirection(i, direction);
            ApplyStartRotation(i, rotation);
        }
    }

    private void OnSliderChanged(int index, float value)
    {
        tempRotations[index] = value;
        ApplyRotation(index, value);
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        tempDirections[index] = isOn;
        SetInputHandlerDirection(index, isOn);
        ApplyRotation(index, sliders[index].value);
    }

    /// <summary>
    /// Live movement: always calls the delta method.
    /// </summary>
    private void ApplyRotation(int index, float value)
    {
        switch (selectedModelIndex)
        {
            case 0:
                var h4 = robotArmInputHandler4Parts;
                if (index == 0) h4.setPart1Rotation(value);
                if (index == 1) h4.setPart2Rotation(value);
                if (index == 2) h4.setPart3Rotation(value);
                break;

            case 1:
                var h5 = robotArmInputHandler5Parts;
                if (index == 0) h5.setPart1Rotation(value);
                if (index == 1) h5.setPart2Rotation(value);
                if (index == 2) h5.setPart3Rotation(value);
                if (index == 3) h5.setPart4Rotation(value);
                break;

            case 2:
                var h5b = robotArmInputHandler5BParts;
                if (index == 0) h5b.setPart1Rotation(value);
                if (index == 1) h5b.setPart2Rotation(value);
                if (index == 2) h5b.setPart3Rotation(value);
                if (index == 3) h5b.setPart4Rotation(value);
                break;

            case 3:
                var h6 = robotArmInputHandler6Parts;
                if (index == 0) h6.setPart1Rotation(value);
                if (index == 1) h6.setPart2Rotation(value);
                if (index == 2) h6.setPart3Rotation(value);
                if (index == 3) h6.setPart4Rotation(value);
                if (index == 4) h6.setPart5Rotation(value);
                break;
        }
    }

    /// <summary>
    /// One‐time base offset: calls the StartRotation API.
    /// </summary>
    private void ApplyStartRotation(int index, float rotation)
    {
        switch (selectedModelIndex)
        {
            case 0:
                var h4 = robotArmInputHandler4Parts;
                if (index == 0) h4.setPart1StartRotation(rotation);
                if (index == 1) h4.setPart2StartRotation(rotation);
                if (index == 2) h4.setPart3StartRotation(rotation);
                break;

            case 1:
                var h5 = robotArmInputHandler5Parts;
                if (index == 0) h5.setPart1StartRotation(rotation);
                if (index == 1) h5.setPart2StartRotation(rotation);
                if (index == 2) h5.setPart3StartRotation(rotation);
                if (index == 3) h5.setPart4StartRotation(rotation);
                break;

            case 2:
                var h5b = robotArmInputHandler5BParts;
                if (index == 0) h5b.setPart1StartRotation(rotation);
                if (index == 1) h5b.setPart2StartRotation(rotation);
                if (index == 2) h5b.setPart3StartRotation(rotation);
                if (index == 3) h5b.setPart4StartRotation(rotation);
                break;

            case 3:
                var h6 = robotArmInputHandler6Parts;
                if (index == 0) h6.setPart1StartRotation(rotation);
                if (index == 1) h6.setPart2StartRotation(rotation);
                if (index == 2) h6.setPart3StartRotation(rotation);
                if (index == 3) h6.setPart4StartRotation(rotation);
                if (index == 4) h6.setPart5StartRotation(rotation);
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

    /// <summary>
    /// Save new bases, reapply them immediately, then hide.
    /// </summary>
    public void ConfirmChanges()
    {
        RefreshModelSettings();

        for (int i = 0; i < activeParts; i++)
        {
            string rotKey = $"{modelName}startRotationpart{i + 1}";
            string dirKey = $"{modelName}directionpart{i + 1}";

            PlayerPrefs.SetFloat(rotKey, tempRotations[i]);
            PlayerPrefs.SetInt(dirKey, tempDirections[i] ? 1 : 0);

            ApplyStartRotation(i, tempRotations[i]);
        }

        PlayerPrefs.Save();
        HidePanel();
        // sliderValueConfig?.LoadSliderValues();

        robotArmSelection.MoveModelAfterStartPosEdit();
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
}
