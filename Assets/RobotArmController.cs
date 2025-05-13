using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for detecting pointer events

public class RobotArmController : MonoBehaviour
{
    // Reference to your simulation script (RobotArmInputHandler)
    public RobotArmInputHandler armInput;

    // Reference to your Bluetooth manager (the plugin script)
    public BluetoothManager bluetoothManager;

    // Reference to UI sliders
    public Slider sliderBase, sliderJoint1, sliderJoint2;

    private void Start()
    {
        // Ensure sliders update the model instantly when moved
        sliderBase.onValueChanged.AddListener(OnBaseRotationChanged);
        sliderJoint1.onValueChanged.AddListener(OnJoint1RotationChanged);
        sliderJoint2.onValueChanged.AddListener(OnJoint2RotationChanged);

        // Detect when user releases the slider
        sliderBase.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnBaseRotationReleased));
        sliderJoint1.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnJoint1RotationReleased));
        sliderJoint2.GetComponent<EventTrigger>().triggers.Add(CreatePointerUpTrigger(OnJoint2RotationReleased));
    }

    // --- Model Updates (Executed on every value change) ---
    public void OnBaseRotationChanged(float value)
    {
        armInput.SetBaseRotation(value); // Updates model instantly
    }

    public void OnJoint1RotationChanged(float value)
    {
        armInput.SetJoint1Rotation(value);
    }

    public void OnJoint2RotationChanged(float value)
    {
        armInput.SetJoint2Rotation(value);
    }

    // --- Send Final Value (Executed when user stops sliding) ---
    public void OnBaseRotationReleased()
    {
        string command = "STEPPER:" + Mathf.Round(sliderBase.value).ToString();
        bluetoothManager.WriteData(command);
    }

    public void OnJoint1RotationReleased()
    {
        string command = "SERVO1:" + Mathf.Round(sliderJoint1.value).ToString();
        bluetoothManager.WriteData(command);
    }

    public void OnJoint2RotationReleased()
    {
        string command = "SERVO2:" + Mathf.Round(sliderJoint2.value).ToString();
        bluetoothManager.WriteData(command);
    }

    // Utility method to create PointerUp event triggers dynamically
    private EventTrigger.Entry CreatePointerUpTrigger(UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entry.callback.AddListener((data) => action.Invoke());
        return entry;
    }

    // --- Button Event Methods (UNCHANGED) ---
    public void OnSetJointsButtonPressed()
    {
        if (armInput == null)
        {
            Debug.LogError("armInput is not assigned!");
            return; // Stop execution to prevent further errors
        }

        if (bluetoothManager == null)
        {
            Debug.LogError("bluetoothManager is not assigned!");
            return; // Stop execution to prevent further errors
        }

        armInput.SetJoints3Position();
        bluetoothManager.WriteData("SERVO3:177");

        PlayerPrefs.SetInt("SetButtonPressed", 1);
        PlayerPrefs.SetInt("ResetButtonPressed", 0);
        PlayerPrefs.Save();
    }

    public void OnResetJointsButtonPressed()
    {
        armInput.ResetJoints3Position();
        bluetoothManager.WriteData("SERVO3:105");

        PlayerPrefs.SetInt("SetButtonPressed", 0);
        PlayerPrefs.SetInt("ResetButtonPressed", 1);
        PlayerPrefs.Save();
    }

    public void OnBluetoothButtonPressed()
    {
        bluetoothManager.ShowBluetoothPanel();
    }

    // --- Navigation Buttons (UNCHANGED) ---
    public void OnNavigateToServoControl()
    {
        Debug.Log("Navigating to Servo Control screen.");
    }

    public void OnNavigateToSavedPositions()
    {
        Debug.Log("Navigating to Saved Positions screen.");
    }
}
