using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;

public class CategorySelectionManager : MonoBehaviour
{
    [Header("Sprites for Toggle Positions")]
    public Sprite spriteSolo;
    public Sprite spriteFirst;
    public Sprite spriteMiddle;
    public Sprite spriteLast;

    [Header("Category Icons (4)")]
    public Sprite categoryIcon0;
    public Sprite categoryIcon1;
    public Sprite categoryIcon2;
    public Sprite categoryIcon3;

    [Header("Prefabs & UI")]
    public GameObject toggleItemPrefab;     // Prefab with CategoryToggleItem attached
    public LeanToggle selectAllToggle;      // Select All toggle (LeanToggle)
    public Transform toggleContainer;       // Parent transform for toggles
    public Button confirmButton;

    [Header("Reference to Backup Logic")]
    public PlayerPrefsBackup prefsBackup;

    // Hardcoded titles and descriptions in code (must match enum order!)
    private readonly string[] categoryTitles = new string[]
    {
        "Command Construct",
        "Encryption",
        "Slider Config",
        "Visual Config"
    };

    private readonly string[] categoryDescriptions = new string[]
    {
        "Command Configurations",
        "Encryption Type and Keys",
        "Slider Configurations",
        "3d Model Configurations"
    };

    [Serializable]
    public class ItemData
    {
        public string itemName;
        public PrefCategory prefCategory;
        public int category; // icon index
    }

    private List<ItemData> availableItems = new List<ItemData>();
    private List<CategoryToggleItem> toggleItems = new List<CategoryToggleItem>();

    private Sprite[] categorySprites;
    private bool isRestoreMode = false;
    private string restoreFilePath;
    private List<PrefCategory> restoreAvailableCategories;

    private void Awake()
    {
        categorySprites = new Sprite[] { categoryIcon0, categoryIcon1, categoryIcon2, categoryIcon3 };

        if (selectAllToggle != null)
        {
            selectAllToggle.OnOn.RemoveAllListeners();
            selectAllToggle.OnOff.RemoveAllListeners();

            selectAllToggle.OnOn.AddListener(OnSelectAllOn);
            selectAllToggle.OnOff.AddListener(OnSelectAllOff);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirm);
        }
    }

    private void OnSelectAllOn()
    {
        OnSelectAllToggleChanged(true);
    }

    private void OnSelectAllOff()
    {
        OnSelectAllToggleChanged(false);
    }

    public void RefreshToggleList(List<ItemData> newAvailableItems)
    {
        availableItems.Clear();
        toggleItems.Clear();

        // Sort incoming items by enum order (PrefCategory enum order)
        var orderedItems = newAvailableItems.OrderBy(item => (int)item.prefCategory).ToList();

        // Assign sorted list to availableItems
        availableItems = orderedItems;

        // Clear existing toggles from container
        foreach (Transform child in toggleContainer)
        {
            Destroy(child.gameObject);
        }

        if (availableItems.Count == 0) return;

        int enumCount = Enum.GetValues(typeof(PrefCategory)).Length;

        // Defensive checks
        if (categoryTitles.Length != enumCount)
        {
            Debug.LogWarning("[CategorySelectionManager] categoryTitles length does not match PrefCategory enum count.");
        }
        if (categoryDescriptions.Length != enumCount)
        {
            Debug.LogWarning("[CategorySelectionManager] categoryDescriptions length does not match PrefCategory enum count.");
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            var data = availableItems[i];
            GameObject go = Instantiate(toggleItemPrefab, toggleContainer);
            go.name = "ToggleItem_" + data.itemName;

            var toggleItem = go.GetComponent<CategoryToggleItem>();
            if (toggleItem == null)
            {
                Debug.LogError("[CategorySelectionManager] Toggle prefab missing CategoryToggleItem component!");
                continue;
            }

            int iconIndex = (int)data.prefCategory;

            string title = (iconIndex >= 0 && iconIndex < categoryTitles.Length) ? categoryTitles[iconIndex] : data.itemName;
            string description = (iconIndex >= 0 && iconIndex < categoryDescriptions.Length) ? categoryDescriptions[iconIndex] : "";

            toggleItem.Setup(
                title,
                description,
                data.prefCategory,
                iconIndex,
                categorySprites,
                spriteSolo, spriteFirst, spriteMiddle, spriteLast,
                i, availableItems.Count
            );

            toggleItem.OnToggleChanged += _ => UpdateSelectAllToggleStatus();

            toggleItems.Add(toggleItem);
        }

        UpdateSelectAllToggleStatus();
    }

    private void OnSelectAllToggleChanged(bool isOn)
    {
        foreach (var toggleItem in toggleItems)
        {
            if (toggleItem.toggle.enabled && toggleItem.IsOn() != isOn)
            {
                toggleItem.SetOn(isOn);
            }
        }
    }

    private void UpdateSelectAllToggleStatus()
    {
        if (selectAllToggle == null) return;

        bool allOn = toggleItems.All(t => t.toggle.enabled && t.IsOn());

        // Temporarily remove listeners to avoid recursion
        selectAllToggle.OnOn.RemoveAllListeners();
        selectAllToggle.OnOff.RemoveAllListeners();

        selectAllToggle.Set(allOn);

        selectAllToggle.OnOn.AddListener(OnSelectAllOn);
        selectAllToggle.OnOff.AddListener(OnSelectAllOff);
    }

    public List<PrefCategory> GetSelectedCategories()
    {
        var selected = new List<PrefCategory>();
        for (int i = 0; i < toggleItems.Count; i++)
        {
            if (toggleItems[i].toggle.enabled && toggleItems[i].IsOn())
            {
                selected.Add(availableItems[i].prefCategory);
            }
        }
        return selected;
    }

    public void ShowBackupPanel()
    {
        isRestoreMode = false;
        ResetAllToggles();
        gameObject.SetActive(true);
    }

    public void ShowRestorePanel(string filePath, List<PrefCategory> availableCategories)
    {
        isRestoreMode = true;
        restoreFilePath = filePath;
        restoreAvailableCategories = availableCategories;

        ResetAllToggles();
        SetAvailableCategories(availableCategories);
        gameObject.SetActive(true);
    }

    private void SetAvailableCategories(List<PrefCategory> available)
    {
        for (int i = 0; i < toggleItems.Count; i++)
        {
            bool isAvailable = available.Contains(availableItems[i].prefCategory);

            toggleItems[i].SetInteractable(isAvailable);
            toggleItems[i].SetOn(isAvailable);
            toggleItems[i].gameObject.SetActive(isAvailable);
        }
        UpdateSelectAllToggleStatus();
    }

    private void ResetAllToggles()
    {
        foreach (var toggleItem in toggleItems)
        {
            toggleItem.SetOn(false);
            toggleItem.SetInteractable(true);
            toggleItem.gameObject.SetActive(true);
        }
        if (selectAllToggle != null)
            selectAllToggle.Set(false);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    private void OnConfirm()
    {
        var selected = GetSelectedCategories();

        if (selected.Count == 0)
        {
            Debug.LogWarning("[CategorySelectionManager] No categories selected.");
            return;
        }

        if (isRestoreMode)
        {
            prefsBackup.RestorePrefsFromFileWithSelectedCategories(restoreFilePath, selected);
            HidePanel(); // Hide entire panel on restore confirm
        }
        else
        {
            prefsBackup.SavePrefsWithSelectedCategories(selected);
            // Only hide backupPanel, not entire backupRestorePanel
            if (prefsBackup != null && prefsBackup.backupPanel != null)
                prefsBackup.backupPanel.SetActive(false);
        }
    }
}
