using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Lean.Gui; // <-- ADDED THIS

public class CustomSaveManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject customSavePanel; // The main panel GameObject
    public TMP_InputField nameInputField; // Input field for the save name
    public Button confirmButton;
    public Button cancelButton;

    // --- MODIFIED: Replaced single Toggle with two LeanToggles ---
    [Header("Claw Toggles (Radio)")]
    public LeanToggle openClawToggle;  // Assign your "Open" LeanToggle
    public LeanToggle closeClawToggle; // Assign your "Close" LeanToggle
    // --- END OF MODIFICATION ---

    [Header("Dynamic Value Fields")]
    // Assign your 5 input fields here in the inspector.
    public List<TMP_InputField> valueInputFields;

    [Header("Manager Link")]
    public SaveManager saveManager; // Assign your existing SaveManager here

    private int currentModelIndex;
    
    // --- ADDED: Fields to manage radio toggle state ---
    private int currentClawState; // 0 = Open, 1 = Close
    private bool isProgrammaticClawToggle;
    // --- END OF ADDITION ---

    void Start()
    {
        if (customSavePanel != null)
            customSavePanel.SetActive(false); // Start hidden

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmPressed);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(HidePanel);

        if (saveManager == null)
            saveManager = FindFirstObjectByType<SaveManager>();
            
        // --- ADDED: Setup radio logic for claw toggles ---
        SetupClawRadioToggles();
        // --- END OF ADDITION ---
    }
    
    /// <summary>
    /// Sets up the listeners for the Open/Close LeanToggles
    /// to behave like radio buttons.
    /// </summary>
    private void SetupClawRadioToggles()
    {
        if (openClawToggle == null || closeClawToggle == null)
        {
            Debug.LogError("CustomSaveManager: Open and Close claw toggles must be assigned!");
            return;
        }

        // We manage turning siblings off manually
        openClawToggle.TurnOffSiblings = false;
        closeClawToggle.TurnOffSiblings = false;

        // Add 'OnOn' listeners (when they are turned ON)
        openClawToggle.OnOn.AddListener(() => SelectClawState(0)); // 0 = Open
        closeClawToggle.OnOn.AddListener(() => SelectClawState(1)); // 1 = Close

        // Add 'OnOff' listeners to prevent de-selecting the active one
        openClawToggle.OnOff.AddListener(() =>
        {
            // If this was the active toggle (0) and we didn't turn it off via code,
            // force it back on.
            if (currentClawState == 0 && !isProgrammaticClawToggle)
            {
                openClawToggle.On = true;
            }
        });
        
        closeClawToggle.OnOff.AddListener(() =>
        {
            // If this was the active toggle (1) and we didn't turn it off via code,
            // force it back on.
            if (currentClawState == 1 && !isProgrammaticClawToggle)
            {
                closeClawToggle.On = true;
            }
        });
    }

    /// <summary>
    /// This is the public method you should call from your new 
    /// "Create Custom Save" button's OnClick() event.
    /// </summary>
    public void OpenCustomSavePanel()
    {
        if (saveManager == null)
        {
            Debug.LogError("SaveManager is not assigned to CustomSaveManager!");
            return;
        }

        // --- 1. Get model index and required field count ---
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        int requiredFields = GetRequiredFieldCount(currentModelIndex);

        // --- 2. Show the correct number of input fields ---
        SetupInputFields(requiredFields);

        // --- 3. Reset generic fields ---
        nameInputField.text = "";
        
        // --- MODIFIED: Set default state using new method ---
        SelectClawState(0, playTransitions: false); // Default to "Open" (state 0)
        // --- END OF MODIFICATION ---

        // --- 4. Pre-fill values from PlayerPrefs ---
        for (int i = 0; i < valueInputFields.Count; i++)
        {
            if (valueInputFields[i] == null) continue;
            
            bool isFieldRequired = (i < requiredFields);

            if (isFieldRequired)
            {
                string prefKey = $"Slider{i + 1}_Start";
                int startValue = PlayerPrefs.GetInt(prefKey, 90);
                valueInputFields[i].text = startValue.ToString();
            }
        }

        // --- 5. Show the panel ---
        customSavePanel.SetActive(true);

        // --- 6. Register with PanelManager (if you use it) ---
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PushPanel(
                key: customSavePanel,
                hide: HidePanel,
                isActive: IsPanelActive
            );
        }
    }
    
    /// <summary>
    /// Core logic to set the active claw state (0=Open, 1=Close).
    /// </summary>
    private void SelectClawState(int newState, bool playTransitions = true)
    {
        // Ignore clicking the already-active toggle
        if (newState == currentClawState && playTransitions)
            return;

        currentClawState = newState;
        
        // We are now programmatically changing toggles
        isProgrammaticClawToggle = true; 

        if (newState == 0) // Select "Open"
        {
            // Turn ON the Open toggle
            if (playTransitions)
                openClawToggle.TurnOn();
            else
                openClawToggle.On = true;
            
            // Turn OFF the Close toggle
            closeClawToggle.TurnOff();
        }
        else // Select "Close" (newState == 1)
        {
            // Turn ON the Close toggle
            if (playTransitions)
                closeClawToggle.TurnOn();
            else
                closeClawToggle.On = true;
            
            // Turn OFF the Open toggle
            openClawToggle.TurnOff();
        }

        // We are done programmatically changing toggles
        isProgrammaticClawToggle = false; 
    }

    /// <summary>
    /// Shows/hides the value input fields based on the required count.
    /// </summary>
    private void SetupInputFields(int requiredFields) 
    {
        if (valueInputFields == null || valueInputFields.Count < 5)
        {
            Debug.LogError("Not enough input fields (expected 5) assigned in CustomSaveManager!");
            return;
        }

        for (int i = 0; i < valueInputFields.Count; i++)
        {
            if (valueInputFields[i] != null)
            {
                Transform parentObject = valueInputFields[i].gameObject.transform.parent;
                if (parentObject != null)
                {
                    parentObject.gameObject.SetActive(i < requiredFields);
                }
                else
                {
                    valueInputFields[i].gameObject.SetActive(i < requiredFields);
                }
            }
        }
    }

    /// <summary>
    /// Logic to run when the "Confirm" button is pressed.
    /// </summary>
    private void OnConfirmPressed()
    {
        // --- 1. Get Name and Existing Names ---
        string baseName = nameInputField.text.Trim();
        HashSet<string> existingNames = saveManager.GetAllSaveNames(currentModelIndex);
        
        string saveName;

        // --- 2. Generate Unique Name ---
        if (string.IsNullOrWhiteSpace(baseName))
        {
            saveName = saveManager.GenerateDefaultName();
        }
        else
        {
            saveName = saveManager.GenerateUniqueName(baseName, existingNames);
        }

        // --- 3. Validation: Parse Values ---
        List<int> saveValues = new List<int>();
        int requiredFields = GetRequiredFieldCount(currentModelIndex);

        for (int i = 0; i < requiredFields; i++)
        {
            if (!int.TryParse(valueInputFields[i].text, out int value))
            {
                saveManager.Toast($"Invalid value in field {i + 1}. Please enter numbers only.");
                return;
            }
            saveValues.Add(value);
        }

        // --- 4. MODIFIED: Add Claw State ---
        // Get the state from our internal variable
        int clawState = currentClawState; // 0 for Open, 1 for Close
        // --- END OF MODIFICATION ---
        
        saveValues.Add(clawState);

        // --- 5. Call SaveManager to save the data ---
        saveManager.SaveCustomArray(currentModelIndex, saveName, saveValues);

        saveManager.Toast("Saved as: " + saveName);
        HidePanel();
    }

    /// <summary>
    /// Helper to get the number of fields for the model.
    /// </summary>
    private int GetRequiredFieldCount(int modelIndex)
    {
        return modelIndex switch
        {
            0 => 3,       // Model 0 needs 3 values
            1 or 2 => 4,  // Models 1 and 2 need 4 values
            3 => 5,       // Model 3 needs 5 values
            _ => 3        // Default
        };
    }

    // --- Panel Management Methods ---

    public void HidePanel()
    {
        customSavePanel.SetActive(false);
    }

    public bool IsPanelActive()
    {
        return customSavePanel.activeSelf;
    }
}