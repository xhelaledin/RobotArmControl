using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BluetoothCommandConstructor : MonoBehaviour
{
    public GameObject commandConstructPanel;
    public TMP_InputField slider1CommandInputField;
    public TMP_InputField slider2CommandInputField;
    public TMP_InputField slider3CommandInputField;
    public TMP_InputField slider4CommandInputField;
    public TMP_InputField slider5CommandInputField;
    public TMP_InputField openCommandInputField;
    public TMP_InputField closeCommandInputField;
    public TMP_InputField saveCommandInputField;

    string slider1Command;
    string slider2Command;
    string slider3Command;
    string slider4Command;
    string slider5Command;
    string openCommand;
    string closeCommand;
    string saveCommand;

    public TMP_Dropdown delimiterDropdown;
    string delimiter;

    public BluetoothManager bluetoothManager;

    void Start()
    {
        LoadCommandsFromPrefs();
        delimiterDropdown.onValueChanged.AddListener(OnDelimiterSelected);

        // Optionally sync dropdown UI with saved delimiter
        int savedIndex = GetDelimiterIndex(delimiter);
        delimiterDropdown.value = savedIndex;
        OnDelimiterSelected(savedIndex);
    }

    void OnDelimiterSelected(int index)
    {
        switch (index)
        {
            case 0: delimiter = ":"; break;
            case 1: delimiter = "-"; break;
            case 2: delimiter = "_"; break;
            case 3: delimiter = "."; break;
            case 4: delimiter = ";"; break;
            case 5: delimiter = "+"; break;
            case 6: delimiter = "="; break;
            case 7: delimiter = "~"; break;
        }
    }

    int GetDelimiterIndex(string savedDelimiter)
    {
        string[] options = { ":", "-", "_", ".", ";", "+", "=", "~" };
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == savedDelimiter)
                return i;
        }
        return 0;
    }

    public void OnSetSlider1CommandClicked()
    {
        slider1Command = slider1CommandInputField.text;
        if (string.IsNullOrEmpty(slider1Command)) slider1Command = "S1";
    }
    public void OnSetSlider2CommandClicked()
    {
        slider2Command = slider2CommandInputField.text;
        if (string.IsNullOrEmpty(slider2Command)) slider2Command = "S2";
    }
    public void OnSetSlider3CommandClicked()
    {
        slider3Command = slider3CommandInputField.text;
        if (string.IsNullOrEmpty(slider3Command)) slider3Command = "S3";
    }
    public void OnSetSlider4CommandClicked()
    {
        slider4Command = slider4CommandInputField.text;
        if (string.IsNullOrEmpty(slider4Command)) slider4Command = "S4";
    }
    public void OnSetSlider5CommandClicked()
    {
        slider5Command = slider5CommandInputField.text;
        if (string.IsNullOrEmpty(slider5Command)) slider5Command = "S5";
    }
    public void OnOpenButtonCommandClicked()
    {
        openCommand = openCommandInputField.text;
        if (string.IsNullOrEmpty(openCommand)) openCommand = "OPEN";
    }
    public void OnCloseButtonCommandClicked()
    {
        closeCommand = closeCommandInputField.text;
        if (string.IsNullOrEmpty(closeCommand)) closeCommand = "CLOSE";
    }
    public void OnSaveButtonCommandClicked()
    {
        saveCommand = saveCommandInputField.text;
        if (string.IsNullOrEmpty(saveCommand)) closeCommand = "SAVE";
    }

    public void OnConfirmButtonClicked()
    {
        OnSetSlider1CommandClicked();
        OnSetSlider2CommandClicked();
        OnSetSlider3CommandClicked();
        OnSetSlider4CommandClicked();
        OnSetSlider5CommandClicked();
        OnOpenButtonCommandClicked();
        OnCloseButtonCommandClicked();
        OnSaveButtonCommandClicked();
        SaveCommandsToPrefs();
        HideCommandConstructPanel();
    }

    private void SaveCommandsToPrefs()
    {
        PlayerPrefs.SetString("Slider1_Command", slider1Command);
        PlayerPrefs.SetString("Slider2_Command", slider2Command);
        PlayerPrefs.SetString("Slider3_Command", slider3Command);
        PlayerPrefs.SetString("Slider4_Command", slider4Command);
        PlayerPrefs.SetString("Slider5_Command", slider5Command);
        PlayerPrefs.SetString("Open_Command", openCommand);
        PlayerPrefs.SetString("Close_Command", closeCommand);
        PlayerPrefs.SetString("Save_Command", saveCommand);
        PlayerPrefs.SetString("Command_Delimiter", delimiter);
        PlayerPrefs.Save();
    }


    private void LoadCommandsFromPrefs()
    {
        slider1Command = PlayerPrefs.GetString("Slider1_Command", "S1");
        slider2Command = PlayerPrefs.GetString("Slider2_Command", "S2");
        slider3Command = PlayerPrefs.GetString("Slider3_Command", "S3");
        slider4Command = PlayerPrefs.GetString("Slider4_Command", "S4");
        slider5Command = PlayerPrefs.GetString("Slider5_Command", "S5");
        openCommand = PlayerPrefs.GetString("Open_Command", "OPEN");
        closeCommand = PlayerPrefs.GetString("Close_Command", "CLOSE");
        saveCommand = PlayerPrefs.GetString("Save_Command", "SAVE");
        delimiter = PlayerPrefs.GetString("Command_Delimiter", ":");

        slider1CommandInputField.text = slider1Command;
        slider2CommandInputField.text = slider2Command;
        slider3CommandInputField.text = slider3Command;
        slider4CommandInputField.text = slider4Command;
        slider5CommandInputField.text = slider5Command;

        // slider1CommandInputField.placeholder.GetComponent<TMP_Text>().text = slider1Command;
        // slider2CommandInputField.placeholder.GetComponent<TMP_Text>().text = slider2Command;
        // slider3CommandInputField.placeholder.GetComponent<TMP_Text>().text = slider3Command;
        // slider4CommandInputField.placeholder.GetComponent<TMP_Text>().text = slider4Command;
        // slider5CommandInputField.placeholder.GetComponent<TMP_Text>().text = slider5Command;

        openCommandInputField.text = openCommand;
        closeCommandInputField.text = closeCommand;

        // openCommandInputField.placeholder.GetComponent<TMP_Text>().text = openCommand;
        // closeCommandInputField.placeholder.GetComponent<TMP_Text>().text = closeCommand;

        saveCommandInputField.text = saveCommand;

        // saveCommandInputField.placeholder.GetComponent<TMP_Text>().text = saveCommand;
    }

    public void ConstructSlider1Command(string sliderValue)
    {
        string command = slider1Command + delimiter + sliderValue;
        SendDataBluetooth(command);
    }
    public void ConstructSlider2Command(string sliderValue)
    {
        string command = slider2Command + delimiter + sliderValue;
        SendDataBluetooth(command);
    }
    public void ConstructSlider3Command(string sliderValue)
    {
        string command = slider3Command + delimiter + sliderValue;
        SendDataBluetooth(command);
    }
    public void ConstructSlider4Command(string sliderValue)
    {
        string command = slider4Command + delimiter + sliderValue;
        SendDataBluetooth(command);
    }
    public void ConstructSlider5Command(string sliderValue)
    {
        string command = slider5Command + delimiter + sliderValue;
        SendDataBluetooth(command);
    }
    public void ConstructOpenCommand(string openValue)
    {
        string command = openCommand + delimiter + openValue;
        SendDataBluetooth(command);
    }
    public void ConstructCloseCommand(string closeValue)
    {
        string command = closeCommand + delimiter + closeValue;
        SendDataBluetooth(command);
    }

    public void ConstructSaveCommand(int[] saveValues)
    {
        string command = saveCommand + delimiter;

        for (int i = 0; i < saveValues.Length; i++)
        {
            command += saveValues[i];
            if (i < saveValues.Length - 1)
            {
                command += ",";
            }
        }
        SendDataBluetooth(command);
    }

    public void SendDataBluetooth(string data)
    {
        bluetoothManager.WriteData(data);
    }

    public void ShowCommandConstructPanel()
    {
        commandConstructPanel.SetActive(true);
    }

    public void HideCommandConstructPanel()
    {
        commandConstructPanel.SetActive(false);
    }
}
