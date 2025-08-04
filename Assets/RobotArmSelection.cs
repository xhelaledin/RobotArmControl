using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    public int openValue = 105;
    public int closeValue = 177;

    private void Awake()
    {
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        _sliders = new[]
        {
            slider1,
            slider2,
            slider3,
            slider4,
            slider5
        };
    }

    void Start()
    {
        OnModelSelected(selectedModelIndex);

        slider1.onValueChanged.AddListener(OnSlider1RotationChanged);
        slider2.onValueChanged.AddListener(OnSlider2RotationChanged);
        slider3.onValueChanged.AddListener(OnSlider3RotationChanged);
        slider4.onValueChanged.AddListener(OnSlider4RotationChanged);
        slider5.onValueChanged.AddListener(OnSlider5RotationChanged);

        slider1.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnSlider1RotationReleased));
        slider2.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnSlider2RotationReleased));
        slider3.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnSlider3RotationReleased));
        slider4.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnSlider4RotationReleased));
        slider5.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnSlider5RotationReleased));

        MoveModelByStartValues();
    }

    public void UpdateSelectedModelIndex()
    {
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
    }

    public void ConfigureSliderValue(int index, float min, float max, float start, bool flipDirection)
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
    }

    public void ConfigureOpenCloseValues(int openValue, int closeValue)
    {
        this.openValue = openValue;
        this.closeValue = closeValue;
    }

    // Utility method to create PointerUp event triggers dynamically
    private EventTrigger.Entry CreatePointerUpTrigger(UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entry.callback.AddListener((data) => action.Invoke());
        return entry;
    }

    public void OnModelSelected(int index)
    {
        // Activate the selected model and deactivate the others
        switch (index)
        {
            case 0:  // 4-part model
                armModel4.SetActive(true);
                armModel5.SetActive(false);
                armModel5b.SetActive(false);
                armModel6.SetActive(false);

                // Show only 3 sliders and 2 buttons for 4 parts model
                slider1.gameObject.SetActive(true);
                slider2.gameObject.SetActive(true);
                slider3.gameObject.SetActive(true);
                slider4.gameObject.SetActive(false);
                slider5.gameObject.SetActive(false);

                // openButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                // closeButton.transform.localPosition = new Vector3(200f, 75f, 0f);

                break;

            case 1:  // 5-part model
                armModel4.SetActive(false);
                armModel5.SetActive(true);
                armModel5b.SetActive(false);
                armModel6.SetActive(false);

                // Show only 4 sliders and 2 buttons for 5 parts model
                slider1.gameObject.SetActive(true);
                slider2.gameObject.SetActive(true);
                slider3.gameObject.SetActive(true);
                slider4.gameObject.SetActive(true);
                slider5.gameObject.SetActive(false);

                // openButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                // closeButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                break;

            case 2:  // 5B-part model
                armModel4.SetActive(false);
                armModel5.SetActive(false);
                armModel5b.SetActive(true);
                armModel6.SetActive(false);

                // Show only 4 sliders and 2 buttons for 5B parts model
                slider1.gameObject.SetActive(true);
                slider2.gameObject.SetActive(true);
                slider3.gameObject.SetActive(true);
                slider4.gameObject.SetActive(true);
                slider5.gameObject.SetActive(false);

                // openButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                // closeButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                break;

            case 3:  // 6-part model
                armModel4.SetActive(false);
                armModel5.SetActive(false);
                armModel5b.SetActive(false);
                armModel6.SetActive(true);

                // Show only 5 sliders and 2 buttons for 6 parts model
                slider1.gameObject.SetActive(true);
                slider2.gameObject.SetActive(true);
                slider3.gameObject.SetActive(true);
                slider4.gameObject.SetActive(true);
                slider5.gameObject.SetActive(true);

                // openButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                // closeButton.transform.localPosition = new Vector3(200f, 75f, 0f);
                break;
        }
    }

    public void OnSlider1RotationChanged(float value)
    {
        switch (selectedModelIndex)
        {
            case 0:
                robotArmInputHandler4Parts.setPart1Rotation(value);
                break;
            case 1:
                robotArmInputHandler5Parts.setPart1Rotation(value);
                break;
            case 2:
                robotArmInputHandler5BParts.setPart1Rotation(value);
                break;
            case 3:
                robotArmInputHandler6Parts.setPart1Rotation(value);
                break;
        }
    }
    public void OnSlider2RotationChanged(float value)
    {
        switch (selectedModelIndex)
        {
            case 0:
                robotArmInputHandler4Parts.setPart2Rotation(value);
                break;
            case 1:
                robotArmInputHandler5Parts.setPart2Rotation(value);
                break;
            case 2:
                robotArmInputHandler5BParts.setPart2Rotation(value);
                break;
            case 3:
                robotArmInputHandler6Parts.setPart2Rotation(value);
                break;
        }
    }
    public void OnSlider3RotationChanged(float value)
    {
        switch (selectedModelIndex)
        {
            case 0:
                robotArmInputHandler4Parts.setPart3Rotation(value);
                break;
            case 1:
                robotArmInputHandler5Parts.setPart3Rotation(value);
                break;
            case 2:
                robotArmInputHandler5BParts.setPart3Rotation(value);
                break;
            case 3:
                robotArmInputHandler6Parts.setPart3Rotation(value);
                break;
        }
    }
    public void OnSlider4RotationChanged(float value)
    {
        switch (selectedModelIndex)
        {
            case 0:
                break;
            case 1:
                robotArmInputHandler5Parts.setPart4Rotation(value);
                break;
            case 2:
                robotArmInputHandler5BParts.setPart4Rotation(value);
                break;
            case 3:
                robotArmInputHandler6Parts.setPart4Rotation(value);
                break;
        }
    }
    public void OnSlider5RotationChanged(float value)
    {
        switch (selectedModelIndex)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                robotArmInputHandler6Parts.setPart5Rotation(value);
                break;
        }
    }

    public void OnSlider1RotationReleased()
    {
        bluetoothCommandConstructor.ConstructSlider1Command(Mathf.Round(slider1.value).ToString());
    }
    public void OnSlider2RotationReleased()
    {
        bluetoothCommandConstructor.ConstructSlider2Command(Mathf.Round(slider2.value).ToString());
    }
    public void OnSlider3RotationReleased()
    {
        bluetoothCommandConstructor.ConstructSlider3Command(Mathf.Round(slider3.value).ToString());
    }
    public void OnSlider4RotationReleased()
    {
        bluetoothCommandConstructor.ConstructSlider4Command(Mathf.Round(slider4.value).ToString());
    }
    public void OnSlider5RotationReleased()
    {
        bluetoothCommandConstructor.ConstructSlider5Command(Mathf.Round(slider5.value).ToString());
    }


    public void OnOpenButtonPressed()
    {
        bluetoothCommandConstructor.ConstructOpenCommand(openValue.ToString());

        switch (selectedModelIndex)
        {
            case 0:
                robotArmInputHandler4Parts.OpenClaw();
                break;
            case 1:
                robotArmInputHandler5Parts.OpenClaw();
                break;
            case 2:
                robotArmInputHandler5BParts.OpenClaw();
                break;
            case 3:
                robotArmInputHandler6Parts.OpenClaw();
                break;
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
            case 0:
                robotArmInputHandler4Parts.CloseClaw();
                break;
            case 1:
                robotArmInputHandler5Parts.CloseClaw();
                break;
            case 2:
                robotArmInputHandler5BParts.CloseClaw();
                break;
            case 3:
                robotArmInputHandler6Parts.CloseClaw();
                break;
        }

        PlayerPrefs.SetInt("OpenButtonPressed", 0);
        PlayerPrefs.SetInt("CloseButtonPressed", 1);
        PlayerPrefs.Save();

    }

    public void MoveModelByStartValues()
    {
        int slider1Start = PlayerPrefs.GetInt("Slider1_Start", 90);
        int slider2Start = PlayerPrefs.GetInt("Slider2_Start", 90);
        int slider3Start = PlayerPrefs.GetInt("Slider3_Start", 90);
        int slider4Start = PlayerPrefs.GetInt("Slider4_Start", 90);
        int slider5Start = PlayerPrefs.GetInt("Slider5_Start", 90);

        OnSlider1RotationChanged(slider1Start);
        OnSlider2RotationChanged(slider2Start);
        OnSlider3RotationChanged(slider3Start);
        OnSlider4RotationChanged(slider4Start);
        OnSlider5RotationChanged(slider5Start);
    }

    public void MoveModelAfterStartPosEdit()
    {
        int slider1Value = Mathf.RoundToInt(slider1.value);
        int slider2Value = Mathf.RoundToInt(slider2.value);
        int slider3Value = Mathf.RoundToInt(slider3.value);
        int slider4Value = Mathf.RoundToInt(slider4.value);
        int slider5Value = Mathf.RoundToInt(slider5.value);

        OnSlider1RotationChanged(slider1Value);
        OnSlider2RotationChanged(slider2Value);
        OnSlider3RotationChanged(slider3Value);
        OnSlider4RotationChanged(slider4Value);
        OnSlider5RotationChanged(slider5Value);
    }
}
