using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class CategorySelectionManager : MonoBehaviour
{
    [Header("Category Toggles & Buttons")]
    public Toggle sliderConfigToggle;
    public Button sliderConfigButton;

    public Toggle bluetoothCommandToggle;
    public Button bluetoothCommandButton;

    public Toggle encryptionToggle;
    public Button encryptionButton;

    [Header("Global Toggle")]
    public Toggle selectAllToggle;

    [Header("Panel Buttons")]
    public Button confirmButton;
    public Button cancelButton;

    [Header("Reference to Backup Logic")]
    public PlayerPrefsBackup prefsBackup;

    private Dictionary<PrefCategory, Toggle> categoryToggles;
    private Dictionary<PrefCategory, Button> categoryButtons;

    private bool isRestoreMode = false;
    private string restoreFilePath;
    private List<PrefCategory> restoreAvailableCategories;

    void Awake()
    {
        categoryToggles = new Dictionary<PrefCategory, Toggle>
        {
            { PrefCategory.SliderConfig, sliderConfigToggle },
            { PrefCategory.BluetoothCommandConstruct, bluetoothCommandToggle },
            { PrefCategory.Encryption, encryptionToggle }
        };

        categoryButtons = new Dictionary<PrefCategory, Button>
        {
            { PrefCategory.SliderConfig, sliderConfigButton },
            { PrefCategory.BluetoothCommandConstruct, bluetoothCommandButton },
            { PrefCategory.Encryption, encryptionButton }
        };

        // Button clicks also toggle associated category toggle
        foreach (var kvp in categoryButtons)
        {
            var category = kvp.Key;
            kvp.Value.onClick.AddListener(() => ToggleCategory(category));
        }

        // Each toggle change updates Select All
        foreach (var toggle in categoryToggles.Values)
        {
            toggle.onValueChanged.AddListener(_ => UpdateSelectAllState());
        }

        selectAllToggle.onValueChanged.AddListener(OnSelectAllToggled);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(HidePanel);
    }

    private void ToggleCategory(PrefCategory category)
    {
        if (categoryToggles.TryGetValue(category, out var toggle))
        {
            toggle.isOn = !toggle.isOn;
        }
    }

    private void OnSelectAllToggled(bool isOn)
    {
        foreach (var toggle in categoryToggles.Values)
        {
            if (toggle.interactable)
                toggle.isOn = isOn;
        }
    }

    private void UpdateSelectAllState()
    {
        var interactables = categoryToggles.Values.Where(t => t.interactable).ToList();

        if (interactables.All(t => t.isOn))
            selectAllToggle.SetIsOnWithoutNotify(true);
        else
            selectAllToggle.SetIsOnWithoutNotify(false);
    }

    public List<PrefCategory> GetSelectedCategories()
    {
        return categoryToggles
            .Where(pair => pair.Value.isOn && pair.Value.interactable)
            .Select(pair => pair.Key)
            .ToList();
    }

    public void ShowBackupPanel()
    {
        isRestoreMode = false;
        ResetAll();
        gameObject.SetActive(true);
    }

    public void ShowRestorePanel(string filePath, List<PrefCategory> availableCategories)
    {
        isRestoreMode = true;
        restoreFilePath = filePath;
        restoreAvailableCategories = availableCategories;

        ResetAll();
        SetAvailableCategories(availableCategories);
        gameObject.SetActive(true);
    }

    private void SetAvailableCategories(List<PrefCategory> available)
    {
        foreach (var pair in categoryToggles)
        {
            bool isAvailable = available.Contains(pair.Key);
            pair.Value.interactable = isAvailable;
            pair.Value.isOn = isAvailable;

            // Hide the toggle if not available
            pair.Value.gameObject.SetActive(isAvailable); 
        }

        foreach (var pair in categoryButtons)
        {
            bool isAvailable = available.Contains(pair.Key);
            pair.Value.gameObject.SetActive(isAvailable);
        }

        UpdateSelectAllState();
    }

    private void ResetAll()
    {
        foreach (var pair in categoryToggles)
        {
            pair.Value.SetIsOnWithoutNotify(false);
            pair.Value.interactable = true;
        }

        foreach (var pair in categoryButtons)
        {
            pair.Value.gameObject.SetActive(true);
        }

        selectAllToggle.SetIsOnWithoutNotify(false);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    public void OnConfirm()
    {
        var selected = GetSelectedCategories();

        if (selected.Count == 0)
        {
            Debug.LogWarning("⚠️ No categories selected.");
            return;
        }

        if (isRestoreMode)
        {
            prefsBackup.RestorePrefsFromFileWithSelectedCategories(restoreFilePath, selected);
        }
        else
        {
            prefsBackup.SavePrefsWithSelectedCategories(selected);
        }

        HidePanel();
    }
}
