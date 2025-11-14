using UnityEngine;
using TMPro;
using Lean.Gui;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class BluetoothCommandConstructor : MonoBehaviour, IHideablePanel
{
    [Header("UI Panels")]
    public GameObject commandConstructPanel;

    [Header("Toggle and Dropdowns")]
    public LeanToggle singleModeToggle;

    public AdvancedDropdown commandDelimiterDropdown;
    public AdvancedDropdown listDelimiterDropdown;

    [Header("Input Fields and Display Texts")]
    public TMP_InputField slider1CommandInputField;
    public TMP_Text slider1CommandDisplayText;
    public GameObject slider1EntryContainer;

    public TMP_InputField slider2CommandInputField;
    public TMP_Text slider2CommandDisplayText;
    public GameObject slider2EntryContainer;

    public TMP_InputField slider3CommandInputField;
    public TMP_Text slider3CommandDisplayText;
    public GameObject slider3EntryContainer;

    public TMP_InputField slider4CommandInputField;
    public TMP_Text slider4CommandDisplayText;
    public GameObject slider4EntryContainer;

    public TMP_InputField slider5CommandInputField;
    public TMP_Text slider5CommandDisplayText;
    public GameObject slider5EntryContainer;

    public TMP_InputField openCommandInputField;
    public TMP_Text openCommandDisplayText;

    public TMP_InputField closeCommandInputField;
    public TMP_Text closeCommandDisplayText;

    public TMP_Text closeCommandPrefixText;

    public GameObject openCloseEntryContainer;

    public TMP_InputField saveCommandInputField;
    public TMP_Text saveCommandDisplayText;
    public GameObject saveEntryContainer;

    public GameObject extra1Container;
    public GameObject extra2Container;

    [Header("Bluetooth Manager")]
    public BluetoothManager bluetoothManager;

    [Header("Keyboard Handling")]
    public Canvas worldCanvas;
    public RectTransform commandPanelRect;
    public ScrollRect scrollRect;

    [Header("Debug")]
    public bool debugLogs = false;


    private readonly List<string> delimiterOptions = new List<string> { ":", ",", "-", "_", ".", ";", "+", "=", "~" };

    // Command strings
    private string slider1Command, slider2Command, slider3Command, slider4Command, slider5Command;
    private string openCommand, closeCommand;
    private string saveCommand;

    private string commandDelimiter;
    private string listDelimiter;

    private int lastCommandDelimiterIndex = 0;
    private int lastListDelimiterIndex = 1;

    private void Awake()
    {
        LoadCommandsFromPrefs();
        SyncDropdowns();

        if (singleModeToggle != null)
        {
            singleModeToggle.OnOn.AddListener(UpdateToggleBehavior);
            singleModeToggle.OnOff.AddListener(UpdateToggleBehavior);
        }

        if (commandDelimiterDropdown != null)
            commandDelimiterDropdown.onChangedValue += OnCommandDelimiterChanged;
        if (listDelimiterDropdown != null)
            listDelimiterDropdown.onChangedValue += OnListDelimiterChanged;

        // Setup all input fields
        SetupInputField(slider1CommandInputField, slider1CommandDisplayText, val => slider1Command = val, "S1");
        SetupInputField(slider2CommandInputField, slider2CommandDisplayText, val => slider2Command = val, "S2");
        SetupInputField(slider3CommandInputField, slider3CommandDisplayText, val => slider3Command = val, "S3");
        SetupInputField(slider4CommandInputField, slider4CommandDisplayText, val => slider4Command = val, "S4");
        SetupInputField(slider5CommandInputField, slider5CommandDisplayText, val => slider5Command = val, "S5");

        SetupInputField(openCommandInputField, openCommandDisplayText, val => openCommand = val, "OPEN");
        SetupInputField(closeCommandInputField, closeCommandDisplayText, val => closeCommand = val, "CLOSE");
        SetupInputField(saveCommandInputField, saveCommandDisplayText, val => saveCommand = val, "SAVE");

        UpdateToggleBehavior();
        SyncInputsToDisplay();
        UpdateAllDisplayTexts();
    }


    private void SetupInputField(TMP_InputField inputField, TMP_Text displayText, System.Action<string> onUpdateCommand, string defaultValue)
    {
        if (inputField == null) return;

        // On value change
        inputField.onValueChanged.AddListener(text =>
        {
            if (displayText == saveCommandDisplayText)
            {
                displayText.text = (string.IsNullOrEmpty(text) ? defaultValue : text) + commandDelimiter + BuildSaveExample();
            }
            else
            {
                UpdateDisplayText(displayText, text, defaultValue, "123");
            }

            if (inputField == openCommandInputField && singleModeToggle != null && singleModeToggle.On && closeCommandDisplayText != null)
            {
                string cmd = string.IsNullOrEmpty(text) ? defaultValue : text;
                closeCommandDisplayText.text = cmd + commandDelimiter + "123";
            }
        });

        // On end edit
        inputField.onEndEdit.AddListener(text =>
        {
            string storedValue = string.IsNullOrEmpty(text) ? defaultValue : text;
            onUpdateCommand(storedValue);
            inputField.text = storedValue;

            if (displayText == saveCommandDisplayText)
            {
                displayText.text = storedValue + commandDelimiter + BuildSaveExample();
            }
            else
            {
                UpdateDisplayText(displayText, storedValue, defaultValue, "123");
            }

            SaveCommandsToPrefs();
        });
    }

    private void UpdateDisplayText(TMP_Text displayText, string commandPart, string defaultCommand, string exampleValue)
    {
        if (displayText == null) return;
        string cmd = string.IsNullOrEmpty(commandPart) ? defaultCommand : commandPart;
        displayText.text = cmd + commandDelimiter + exampleValue;
    }

    private void UpdateToggleBehavior()
    {
        bool singleMode = singleModeToggle != null && singleModeToggle.On;

        int selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        // Sliders 1, 2, 3 always active
        if (slider1EntryContainer != null) slider1EntryContainer.SetActive(true);
        if (slider2EntryContainer != null) slider2EntryContainer.SetActive(true);
        if (slider3EntryContainer != null) slider3EntryContainer.SetActive(true);

        // Sliders 4 and 5 depend on model index
        if (slider4EntryContainer != null && slider5EntryContainer != null)
        {
            if (selectedModelIndex == 0)
            {
                slider4EntryContainer.SetActive(false);
                slider5EntryContainer.SetActive(false);
            }
            else if (selectedModelIndex == 1 || selectedModelIndex == 2)
            {
                slider4EntryContainer.SetActive(true);
                slider5EntryContainer.SetActive(false);
            }
            else if (selectedModelIndex == 3)
            {
                slider4EntryContainer.SetActive(true);
                slider5EntryContainer.SetActive(true);
            }
        }

        if (openCloseEntryContainer != null) openCloseEntryContainer.SetActive(true);
        if (saveEntryContainer != null) saveEntryContainer.SetActive(true);

        if (closeCommandInputField != null)
            closeCommandInputField.gameObject.SetActive(!singleMode);

        if (closeCommandPrefixText != null)
            closeCommandPrefixText.text = singleMode ? "Close Cmmnd Prefix: Uses same as open" : "Close Cmmnd Prefix:";

        if (singleMode)
        {
            closeCommand = openCommand;
            if (closeCommandInputField != null) closeCommandInputField.text = openCommand;
            if (closeCommandDisplayText != null) closeCommandDisplayText.text = openCommand + commandDelimiter + "123";
            SaveCommandsToPrefs();
        }

        PlayerPrefs.SetInt("SingleModeToggle", singleMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnCommandDelimiterChanged(int selectedIndex)
    {

        commandDelimiter = delimiterOptions[selectedIndex];
        lastCommandDelimiterIndex = selectedIndex;

        UpdateAllDisplayTexts();
        SaveCommandsToPrefs();
    }

    private void OnListDelimiterChanged(int selectedIndex)
    {

        listDelimiter = delimiterOptions[selectedIndex];
        lastListDelimiterIndex = selectedIndex;

        UpdateAllDisplayTexts();
        SaveCommandsToPrefs();
    }

    private void UpdateAllDisplayTexts()
    {
        UpdateDisplayText(slider1CommandDisplayText, slider1Command, "S1", "123");
        UpdateDisplayText(slider2CommandDisplayText, slider2Command, "S2", "123");
        UpdateDisplayText(slider3CommandDisplayText, slider3Command, "S3", "123");
        UpdateDisplayText(slider4CommandDisplayText, slider4Command, "S4", "123");
        UpdateDisplayText(slider5CommandDisplayText, slider5Command, "S5", "123");

        UpdateDisplayText(openCommandDisplayText, openCommand, "OPEN", "123");

        if (singleModeToggle != null && singleModeToggle.On)
        {
            if (closeCommandDisplayText != null)
                closeCommandDisplayText.text = openCommand + commandDelimiter + "123";
        }
        else
        {
            UpdateDisplayText(closeCommandDisplayText, closeCommand, "CLOSE", "123");
        }

        if (saveCommandDisplayText != null)
            saveCommandDisplayText.text = saveCommand + commandDelimiter + BuildSaveExample();
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

        commandDelimiter = PlayerPrefs.GetString("Command_Delimiter", ":");
        listDelimiter = PlayerPrefs.GetString("List_Delimiter", ",");

        if (commandDelimiter == listDelimiter)
        {
            listDelimiter = ",";
            if (commandDelimiter == ",") commandDelimiter = ":";
        }

        lastCommandDelimiterIndex = GetDelimiterIndex(commandDelimiter);
        lastListDelimiterIndex = GetDelimiterIndex(listDelimiter);

        bool toggleState = PlayerPrefs.GetInt("SingleModeToggle", 0) == 1;
        if (singleModeToggle != null)
            singleModeToggle.On = toggleState;
    }

    private void SyncDropdowns()
    {
        if (commandDelimiterDropdown != null) commandDelimiterDropdown.SelectOption(lastCommandDelimiterIndex);
        if (listDelimiterDropdown != null) listDelimiterDropdown.SelectOption(lastListDelimiterIndex);
    }

    private int GetDelimiterIndex(string delim)
    {
        int idx = delimiterOptions.IndexOf(delim);
        return idx >= 0 ? idx : 0;
    }

    private void SyncInputsToDisplay()
    {
        if (slider1CommandInputField != null) slider1CommandInputField.text = slider1Command;
        if (slider2CommandInputField != null) slider2CommandInputField.text = slider2Command;
        if (slider3CommandInputField != null) slider3CommandInputField.text = slider3Command;
        if (slider4CommandInputField != null) slider4CommandInputField.text = slider4Command;
        if (slider5CommandInputField != null) slider5CommandInputField.text = slider5Command;

        if (openCommandInputField != null) openCommandInputField.text = openCommand;
        if (closeCommandInputField != null) closeCommandInputField.text = closeCommand;

        if (saveCommandInputField != null) saveCommandInputField.text = saveCommand;
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

        PlayerPrefs.SetString("Command_Delimiter", commandDelimiter);
        PlayerPrefs.SetString("List_Delimiter", listDelimiter);

        PlayerPrefs.SetInt("SingleModeToggle", (singleModeToggle != null && singleModeToggle.On) ? 1 : 0);

        PlayerPrefs.Save();
    }

    // Command constructors for BluetoothManager
    public void ConstructSlider1Command(string sliderValue) => SendDataBluetooth(slider1Command + commandDelimiter + sliderValue);
    public void ConstructSlider2Command(string sliderValue) => SendDataBluetooth(slider2Command + commandDelimiter + sliderValue);
    public void ConstructSlider3Command(string sliderValue) => SendDataBluetooth(slider3Command + commandDelimiter + sliderValue);
    public void ConstructSlider4Command(string sliderValue) => SendDataBluetooth(slider4Command + commandDelimiter + sliderValue);
    public void ConstructSlider5Command(string sliderValue) => SendDataBluetooth(slider5Command + commandDelimiter + sliderValue);

    public void ConstructOpenCommand(string openValue)
    {
        if (singleModeToggle != null && singleModeToggle.On)
        {
            closeCommand = openCommand;
            SaveCommandsToPrefs();
        }
        SendDataBluetooth(openCommand + commandDelimiter + openValue);
    }

    public void ConstructCloseCommand(string closeValue)
    {
        string cmd = (singleModeToggle != null && singleModeToggle.On) ? openCommand : closeCommand;
        SendDataBluetooth(cmd + commandDelimiter + closeValue);
    }

    public void ConstructSaveCommand(int[] saveValues)
    {
        string command = saveCommand + commandDelimiter;
        for (int i = 0; i < saveValues.Length; i++)
        {
            command += saveValues[i];
            if (i < saveValues.Length - 1)
                command += listDelimiter;
        }
        SendDataBluetooth(command);
    }

    private string BuildSaveExample()
    {
        int selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        int numValues = selectedModelIndex == 0 ? 4 : (selectedModelIndex == 3 ? 6 : 5);

        string saveExample = "";
        for (int i = 0; i < numValues; i++)
        {
            saveExample += "123";
            if (i < numValues - 1)
                saveExample += listDelimiter;
        }
        return saveExample;
    }


    private void SendDataBluetooth(string data)
    {
        if (bluetoothManager != null)
            bluetoothManager.WriteData(data);
    }

    // Show/hide command panel and slider visibility based on model index (example)
    public void ShowCommandConstructPanel()
    {
        if (commandConstructPanel != null) commandConstructPanel.SetActive(true);

        int selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        if (slider1EntryContainer != null) slider1EntryContainer.SetActive(true);
        if (slider2EntryContainer != null) slider2EntryContainer.SetActive(true);
        if (slider3EntryContainer != null) slider3EntryContainer.SetActive(true);

        if (slider4EntryContainer != null && slider5EntryContainer != null)
        {
            if (selectedModelIndex == 0)
            {
                slider4EntryContainer.SetActive(false);
                slider5EntryContainer.SetActive(false);
            }
            else if (selectedModelIndex == 1 || selectedModelIndex == 2)
            {
                slider4EntryContainer.SetActive(true);
                slider5EntryContainer.SetActive(false);
            }
            else if (selectedModelIndex == 3)
            {
                slider4EntryContainer.SetActive(true);
                slider5EntryContainer.SetActive(true);
            }
        }

        // Update save command display when panel is shown
        if (saveCommandDisplayText != null)
            saveCommandDisplayText.text = saveCommand + commandDelimiter + BuildSaveExample();

        PanelManager.Instance.RegisterPanel(this);
    }

    public void HidePanel()
    {
        commandConstructPanel.SetActive(false);
    }

    public bool IsPanelActive()
    {
        return commandConstructPanel.activeSelf;
    }
}
