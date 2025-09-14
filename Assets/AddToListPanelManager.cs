using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Panel for adding a save to one or multiple lists.
/// </summary>
public class AddToListPanelManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Button closeButton;
    public Button cancelButton;
    public Button confirmButton;           // Confirm button to add selected lists
    public GameObject listItemPrefab;      // Must include Toggle + TMP_Text + AddToListItemManager
    public Transform listContent;

    [Header("Create New Item Prefab")]
    public GameObject createNewItemPrefab; // Prefab for "Create New" button in list

    [Header("Rename Popup")]
    public GameObject renamePopup;
    public TMP_InputField renameInputField;
    public Button renameCancelButton;
    public Button renameConfirmButton;

    [Header("Managers")]
    [SerializeField] private SaveListManager saveListManager;

    [Header("Defaults")]
    public string defaultListName = "New List";

    private string saveToAdd;
    private readonly List<AddToListItemManager> toggleItems = new();

    private void Awake()
    {
        if (saveListManager == null)
            saveListManager = FindFirstObjectByType<SaveListManager>();

        if (panel != null) panel.SetActive(false);
        if (renamePopup != null) renamePopup.SetActive(false);

        // Hook up buttons
        closeButton?.onClick.AddListener(() => panel.SetActive(false));
        cancelButton?.onClick.AddListener(() => panel.SetActive(false));
        renameCancelButton?.onClick.AddListener(() => renamePopup.SetActive(false));
        renameConfirmButton?.onClick.AddListener(ConfirmCreateNewList);
        confirmButton?.onClick.AddListener(AddToSelectedLists);
    }

    /// <summary>
    /// Open the panel for a specific save name.
    /// </summary>
    public void Open(string saveName)
    {
        saveToAdd = saveName;
        panel.SetActive(true);
        RefreshList();
    }

    /// <summary>
    /// Refresh the list of all available save lists.
    /// </summary>
    private void RefreshList()
    {
        if (saveListManager == null || listItemPrefab == null || createNewItemPrefab == null) return;
        toggleItems.Clear();

        // Clear existing items
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        var lists = saveListManager.GetAllListsForCurrentModel();
        if (lists == null) return;

        // Add "Create New" button at top
        GameObject createNewGO = Instantiate(createNewItemPrefab, listContent);
        AddToListItemManager createManager = createNewGO.GetComponent<AddToListItemManager>();
        createManager.SetAsCreateNew(OpenRenamePopup);
        toggleItems.Add(createManager);

        // Add each list as a toggle
        foreach (var kvp in lists)
        {
            GameObject item = Instantiate(listItemPrefab, listContent);
            AddToListItemManager manager = item.GetComponent<AddToListItemManager>();
            manager.SetData(kvp.Key);
            toggleItems.Add(manager);
        }
    }

    /// <summary>
    /// Add the save to all selected lists.
    /// </summary>
    private void AddToSelectedLists()
    {
        foreach (var item in toggleItems)
        {
            if (item.IsSelected && !item.IsCreateNew)
                saveListManager.AddSaveToList(saveToAdd, item.ListName, allowDuplicates: true);
        }

        panel.SetActive(false);
    }

    private void OpenRenamePopup()
    {
        if (renamePopup == null || renameInputField == null) return;
        renameInputField.text = "";
        renamePopup.SetActive(true);
    }

    /// <summary>
    /// Create a new list and add the save immediately.
    /// Newly created list is inserted at the top of the toggle list and auto-selected.
    /// Closes the AddToList panel after creation.
    /// </summary>
    private void ConfirmCreateNewList()
    {
        if (renameInputField == null || saveListManager == null || listItemPrefab == null) return;

        string baseName = renameInputField.text.Trim();
        bool isDefault = string.IsNullOrEmpty(baseName);
        if (isDefault) baseName = defaultListName;

        HashSet<string> existingLists = new HashSet<string>(saveListManager.GetAllListsForCurrentModel().Keys);
        string listName = GenerateUniqueName(baseName, existingLists, isDefault);

        // Create the list
        saveListManager.CreateList(listName);

        // Add the save
        saveListManager.AddSaveToList(saveToAdd, listName, allowDuplicates: true);

        // Insert new list toggle at the top (below Create New)
        GameObject item = Instantiate(listItemPrefab, listContent);
        item.transform.SetSiblingIndex(1); // index 0 is Create New
        AddToListItemManager manager = item.GetComponent<AddToListItemManager>();
        manager.SetData(listName);
        manager.Select(); // auto-select
        toggleItems.Insert(1, manager);

        // Close both rename popup and select panel
        renamePopup.SetActive(false);
        panel.SetActive(false);
    }

    private string GenerateUniqueName(string baseName, HashSet<string> existingNames, bool isDefaultName)
    {
        if (!isDefaultName)
        {
            if (!existingNames.Contains(baseName)) return baseName;
            int suffix = 1;
            string newName;
            do { newName = $"{baseName} ({suffix++})"; } while (existingNames.Contains(newName));
            return newName;
        }
        else
        {
            int suffix = 1;
            string newName;
            do { newName = $"{baseName} {suffix++}"; } while (existingNames.Contains(newName));
            return newName;
        }
    }
}
