using UnityEngine;
using TMPro;
using Lean.Gui;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class BluetoothCommandConstructorNew1 : MonoBehaviour
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
    public Canvas worldCanvas;                   // assign if you have world-space or screen-space camera canvas
    public RectTransform commandPanelRect;
    public ScrollRect scrollRect;                // IMPORTANT: assign your ScrollRect here

    [Header("Debug")]
    public bool debugLogs = false;

    private Vector3 commandPanelOriginalPos;
    private Vector2 contentOriginalAnchoredPos;  // to restore scroll when keyboard hides
    private Coroutine keyboardCoroutine;

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
        // store original UI pos
        commandPanelOriginalPos = commandPanelRect != null ? commandPanelRect.localPosition : Vector3.zero;

        if (scrollRect != null && scrollRect.content != null)
            contentOriginalAnchoredPos = scrollRect.content.anchoredPosition;

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

        inputField.onValueChanged.AddListener(text =>
        {
            UpdateDisplayText(displayText, text, defaultValue, "123");
        });

        inputField.onEndEdit.AddListener(text =>
        {
            string storedValue = string.IsNullOrEmpty(text) ? defaultValue : text;
            onUpdateCommand(storedValue);
            inputField.text = storedValue;
            UpdateDisplayText(displayText, storedValue, defaultValue, "123");
            SaveCommandsToPrefs();
        });

        inputField.onSelect.AddListener(_ =>
        {
            // Start the keyboard coroutine (stop previous if running)
            if (keyboardCoroutine != null) StopCoroutine(keyboardCoroutine);
            keyboardCoroutine = StartCoroutine(WaitAndAdjustForKeyboard(inputField));
        });

        inputField.onDeselect.AddListener(_ =>
        {
            // restore when deselected
            ResetUIPosition();
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
            closeCommandPrefixText.text = singleMode ? "Close Command Prefix: Uses same as open" : "Close Command Prefix:";

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
        if (selectedIndex == lastListDelimiterIndex)
        {
            ShowToast("You can't use the same delimiter for both!");
            if (commandDelimiterDropdown != null) commandDelimiterDropdown.SelectOption(lastCommandDelimiterIndex);
            return;
        }

        commandDelimiter = delimiterOptions[selectedIndex];
        lastCommandDelimiterIndex = selectedIndex;

        UpdateAllDisplayTexts();
        SaveCommandsToPrefs();
    }

    private void OnListDelimiterChanged(int selectedIndex)
    {
        if (selectedIndex == lastCommandDelimiterIndex)
        {
            ShowToast("You can't use the same delimiter for both!");
            if (listDelimiterDropdown != null) listDelimiterDropdown.SelectOption(lastListDelimiterIndex);
            return;
        }

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
            UpdateDisplayText(closeCommandDisplayText, closeCommand, "CLOSE", "123");

        string saveExample = "123" + listDelimiter + "123" + listDelimiter + "123" + listDelimiter + "123" + listDelimiter + "213" + listDelimiter + "123";
        if (saveCommandDisplayText != null)
            saveCommandDisplayText.text = saveCommand + commandDelimiter + saveExample;
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
    }

    public void HideCommandConstructPanel() => commandConstructPanel.SetActive(false);

    private void ShowToast(string message)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("Toast: " + message);
            return;
        }

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, 0);
            toastObject.Call("show");
        }));
    }

    // ------------------------------
    // Keyboard / Scroll handling
    // ------------------------------

    // Coroutine started when an input is selected. Runs while any input stays focused.
    private IEnumerator WaitAndAdjustForKeyboard(TMP_InputField focusedInput)
    {
        // let layout settle
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        // store original content anchored pos (so we can restore)
        if (scrollRect != null && scrollRect.content != null)
            contentOriginalAnchoredPos = scrollRect.content.anchoredPosition;

        // Loop while any input is focused
        while (IsAnyInputFocused())
        {
            // find the current focused input (could have changed)
            TMP_InputField current = GetFocusedInputField();

            if (current == null)
            {
                yield return null;
                continue;
            }

            // if (current == openCommandInputField ||
            //     current == closeCommandInputField ||
            //     current == saveCommandInputField)
            // {
            //     AddExtraItems(true);
            //     // ✅ Your special handling here
            //     Debug.Log("Special field focused: " + current.name);
            // }
            // // else
            // // {
            // //     AddExtraItems(false);
            // // }

            // Find the container which is a direct child of content (the "entry")
            RectTransform entry = FindEntryContainerForInput(current);
            if (entry == null)
            {
                // fallback - try parent
                if (current.transform.parent is RectTransform rt) entry = rt;
            }

            // get keyboard rect in screen coords
            Rect kb = GetKeyboardScreenRect();

            if (kb.height > 0f)
            {
                float keyboardTopY = kb.y + kb.height; // screen coords (bottom-left origin)

                // Wait for one frame to ensure the UI layouts are updated before measuring
                Canvas.ForceUpdateCanvases();
                yield return new WaitForEndOfFrame();

                // do the precise scroll
                ScrollToContainerWithKeyboard(entry, keyboardTopY, 8f); // margin = 8 px
            }
            else
            {
                // Keyboard not visible yet - ensure container is generally visible (soft fallback)
                Canvas.ForceUpdateCanvases();
                yield return new WaitForEndOfFrame();
                ScrollToContainerSimple(entry, 0.18f);
            }

            yield return null;
        }

        // restore original anchored pos when keyboard hides / input loses focus
        if (scrollRect != null && scrollRect.content != null)
            scrollRect.content.anchoredPosition = contentOriginalAnchoredPos;

        keyboardCoroutine = null;
    }


    // Find the nearest child RectTransform of scrollRect.content that contains the input.
    private RectTransform FindEntryContainerForInput(TMP_InputField input)
    {
        if (input == null || scrollRect == null || scrollRect.content == null) return null;

        RectTransform t = input.transform as RectTransform;

        // Walk up until we find a direct child of scrollRect.content (common structure)
        while (t != null && t != scrollRect.content)
        {
            if (t.parent == scrollRect.content)
                return t;
            t = t.parent as RectTransform;
        }

        // Fallback: if nothing found, check if the original transform is under content in any depth.
        // If so, return the top-most child of content that is ancestor of input.
        if (input.transform.IsChildOf(scrollRect.content))
        {
            // iterate content children
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                RectTransform child = scrollRect.content.GetChild(i) as RectTransform;
                if (child != null && input.transform.IsChildOf(child))
                    return child;
            }
        }

        return null;
    }

    // Attempts to obtain the keyboard visible rect in screen coordinates (bottom-left origin).
    // Uses TouchScreenKeyboard.area when available; falls back to Android JNI method.
    private Rect GetKeyboardScreenRect()
    {
        // Try TouchScreenKeyboard if available & has area
        try
        {
            if (TouchScreenKeyboard.visible)
            {
                Rect area = TouchScreenKeyboard.area;
                if (area.height > 0f)
                {
                    if (debugLogs) Debug.Log("[KB] TouchScreenKeyboard.area: " + area);
                    return area;
                }
            }
        }
        catch { /* ignore */ }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android JNI fallback - compute visible display frame and deduce keyboard height
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
            using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
            {
                AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
                decorView.Call("getWindowVisibleDisplayFrame", rect);

                int visibleTop = rect.Call<int>("top"); // top pixel (y from top)
                int visibleBottom = rect.Call<int>("bottom"); // bottom pixel from top
                // Android window coords origin at top-left; Unity screens at bottom-left.
                int visibleHeight = visibleBottom - visibleTop;
                int totalHeight = Screen.height;
                int keyboardHeight = totalHeight - visibleHeight;

                if (keyboardHeight > totalHeight * 0.12f)
                {
                    // set keyboard rect bottom at screen bottom (y=0), height = keyboardHeight
                    Rect kb = new Rect(0, 0, Screen.width, keyboardHeight);
                    if (debugLogs) Debug.Log($"[KB] JNI visibleHeight={visibleHeight} total={totalHeight} kbH={keyboardHeight}");
                    return kb;
                }
            }
        }
        catch (System.Exception ex)
        {
            if (debugLogs) Debug.LogWarning("[KB] JNI fail: " + ex.Message);
        }
#endif

        // not visible / unknown
        return new Rect(0, 0, 0, 0);
    }

    // Scroll so entry bottom is just above keyboardTopScreenY (in screen pixels). marginPixels is extra space above keyboard.
    private void ScrollToContainerWithKeyboard(RectTransform entry, float keyboardTopScreenY, float marginPixels)
    {
        if (scrollRect == null || scrollRect.content == null || scrollRect.viewport == null || entry == null) return;

        // Determine camera to use when converting screen <-> local
        Canvas viewportCanvas = scrollRect.viewport.GetComponentInParent<Canvas>();
        Camera camForConversion = null;
        if (viewportCanvas != null)
        {
            camForConversion = (viewportCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : viewportCanvas.worldCamera;
        }
        else
        {
            camForConversion = (worldCanvas != null && worldCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? worldCanvas.worldCamera : null;
        }

        // Get world bottom corner of entry
        Vector3[] corners = new Vector3[4];
        entry.GetWorldCorners(corners);
        Vector3 bottomWorld = corners[0]; // bottom-left
        Vector2 bottomScreen = RectTransformUtility.WorldToScreenPoint(camForConversion, bottomWorld);

        // keyboard top screen point (we'll keep X the same as bottomScreen.x)
        Vector2 kbTopScreen = new Vector2(bottomScreen.x, keyboardTopScreenY);

        // Convert both to viewport local coordinates (same coordinate space)
        Vector2 bottomLocal, kbTopWithMarginLocal;

        bool okBottom = RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRect.viewport, bottomScreen, camForConversion, out bottomLocal);
        bool okKb = RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRect.viewport, new Vector2(kbTopScreen.x, kbTopScreen.y + marginPixels), camForConversion, out kbTopWithMarginLocal);

        if (!okBottom || !okKb)
        {
            if (debugLogs) Debug.Log("[Scroll] ScreenToLocal failed");
            return;
        }

        // If bottomLocal.y already above the target, nothing to do
        float targetLocalY = kbTopWithMarginLocal.y; // where bottom should be
        if (bottomLocal.y >= targetLocalY)
        {
            if (debugLogs) Debug.Log($"[Scroll] already above target: bottomLocal.y={bottomLocal.y} target={targetLocalY}");
            return;
        }

        // How much to move content in local units
        float deltaLocal = targetLocalY - bottomLocal.y; // positive means we need to move content up

        // Compute new anchoredPosition
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        // content.anchoredPosition.y corresponds to the vertical offset: we will add deltaLocal
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // clamp range: anchoredPosition.y should be within [0, maxScroll]
        float maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);

        // Current anchored y
        float curY = content.anchoredPosition.y;

        float newY = curY + deltaLocal;

        // clamp
        newY = Mathf.Clamp(newY, 0f, maxScroll);

        // Smooth the movement a bit - immediate set gives best UX for keyboard but we can Lerp small if desired
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

        if (debugLogs) Debug.Log($"[Scroll] moved content by {deltaLocal} -> newY={newY} (curY={curY})");
    }

    // Gentle fallback: position container bottom near bottomRatio (0..1) of viewport (e.g. 0.18 => ~18% above bottom)
    private void ScrollToContainerSimple(RectTransform entry, float bottomRatio)
    {
        if (scrollRect == null || scrollRect.content == null || scrollRect.viewport == null || entry == null) return;

        Canvas viewportCanvas = scrollRect.viewport.GetComponentInParent<Canvas>();
        Camera camForConversion = null;
        if (viewportCanvas != null)
        {
            camForConversion = (viewportCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : viewportCanvas.worldCamera;
        }
        else
        {
            camForConversion = (worldCanvas != null && worldCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? worldCanvas.worldCamera : null;
        }

        Vector3[] corners = new Vector3[4];
        entry.GetWorldCorners(corners);
        Vector2 bottomScreen = RectTransformUtility.WorldToScreenPoint(camForConversion, corners[0]);

        Vector2 bottomLocal;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRect.viewport, bottomScreen, camForConversion, out bottomLocal)) return;

        float viewportHeight = scrollRect.viewport.rect.height;
        float desiredLocalY = -viewportHeight * 0.5f + viewportHeight * bottomRatio; // viewport local origin is center

        float delta = desiredLocalY - bottomLocal.y;

        RectTransform content = scrollRect.content;
        float contentHeight = content.rect.height;
        float maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);

        float newY = Mathf.Clamp(content.anchoredPosition.y + delta, 0f, maxScroll);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

        if (debugLogs) Debug.Log($"[SimpleScroll] delta={delta}, newY={newY}");
    }

    private TMP_InputField GetFocusedInputField()
    {
        if (slider1CommandInputField != null && slider1CommandInputField.isFocused) return slider1CommandInputField;
        if (slider2CommandInputField != null && slider2CommandInputField.isFocused) return slider2CommandInputField;
        if (slider3CommandInputField != null && slider3CommandInputField.isFocused) return slider3CommandInputField;
        if (slider4CommandInputField != null && slider4CommandInputField.isFocused) return slider4CommandInputField;
        if (slider5CommandInputField != null && slider5CommandInputField.isFocused) return slider5CommandInputField;
        if (openCommandInputField != null && openCommandInputField.isFocused) return openCommandInputField;
        if (closeCommandInputField != null && closeCommandInputField.isFocused) return closeCommandInputField;
        if (saveCommandInputField != null && saveCommandInputField.isFocused) return saveCommandInputField;
        return null;
    }

    private bool IsAnyInputFocused()
    {
        return (slider1CommandInputField != null && slider1CommandInputField.isFocused)
            || (slider2CommandInputField != null && slider2CommandInputField.isFocused)
            || (slider3CommandInputField != null && slider3CommandInputField.isFocused)
            || (slider4CommandInputField != null && slider4CommandInputField.isFocused)
            || (slider5CommandInputField != null && slider5CommandInputField.isFocused)
            || (openCommandInputField != null && openCommandInputField.isFocused)
            || (closeCommandInputField != null && closeCommandInputField.isFocused)
            || (saveCommandInputField != null && saveCommandInputField.isFocused);
    }

    private void ResetUIPosition()
    {
        if (keyboardCoroutine != null)
        {
            StopCoroutine(keyboardCoroutine);
            keyboardCoroutine = null;
        }

        if (commandPanelRect != null)
            commandPanelRect.localPosition = commandPanelOriginalPos;

        if (scrollRect != null && scrollRect.content != null)
            scrollRect.content.anchoredPosition = contentOriginalAnchoredPos;
    }
}
