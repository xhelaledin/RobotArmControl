using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class AddToListPanelManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Button closeButton;
    public Button createNewButton;
    public GameObject listItemPrefab;
    public Transform listContent;

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

    private void Awake()
    {
        if (saveListManager == null)
        {
            saveListManager = FindFirstObjectByType<SaveListManager>();
            if (saveListManager == null)
                Debug.LogError("AddToListPanelManager: No SaveListManager found in scene!");
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(() => panel.SetActive(false));
        if (createNewButton != null)
            createNewButton.onClick.AddListener(OpenRenamePopup);
        if (renameCancelButton != null)
            renameCancelButton.onClick.AddListener(() => renamePopup.SetActive(false));
        if (renameConfirmButton != null)
            renameConfirmButton.onClick.AddListener(ConfirmCreateNewList);

        if (panel != null) panel.SetActive(false);
        if (renamePopup != null) renamePopup.SetActive(false);
    }

    /// <summary>
    /// Open the AddToList panel
    /// </summary>
    public void Open(string saveName)
    {
        saveToAdd = saveName;
        panel.SetActive(true);
        RefreshList();
    }

    private void RefreshList()
    {
        if (saveListManager == null) return;

        // Clear existing items
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        Dictionary<string, SaveListData> lists = saveListManager.GetAllListsForCurrentModel();
        if (lists == null) return;

        foreach (var kvp in lists)
        {
            GameObject item = Instantiate(listItemPrefab, listContent);
            AddToListItemManager manager = item.GetComponent<AddToListItemManager>();
            manager.SetData(kvp.Key, () =>
            {
                // Add same save multiple times (explicitly allowed here)
                saveListManager.AddSaveToList(saveToAdd, kvp.Key, allowDuplicates: true);
                panel.SetActive(false);
            });
        }
    }

    private void OpenRenamePopup()
    {
        if (renamePopup == null || renameInputField == null) return;

        renameInputField.text = "";
        renamePopup.SetActive(true);
    }

    private void ConfirmCreateNewList()
    {
        if (renameInputField == null || saveListManager == null) return;

        string baseName = renameInputField.text.Trim();
        bool isDefault = false;

        if (string.IsNullOrEmpty(baseName))
        {
            baseName = defaultListName;
            isDefault = true;
        }

        HashSet<string> existingLists = new HashSet<string>(saveListManager.GetAllListsForCurrentModel().Keys);
        string listName = GenerateUniqueName(baseName, existingLists, isDefault);

        saveListManager.CreateList(listName);

        // Allow same save multiple times
        saveListManager.AddSaveToList(saveToAdd, listName, allowDuplicates: true);

        renamePopup.SetActive(false);
        panel.SetActive(false);
    }

    /// <summary>
    /// Appends a number to the name if it already exists in the collection.
    /// Default names produce "Base 1", "Base 2", ...
    /// Non-default names produce "Base", "Base (1)", "Base (2)", ...
    /// This function guarantees the returned name is not in existingNames.
    /// </summary>
    private string GenerateUniqueName(string baseName, HashSet<string> existingNames, bool isDefaultName)
    {
        if (!isDefaultName)
        {
            if (!existingNames.Contains(baseName))
                return baseName;

            int suffix = 1;
            string newName;
            do
            {
                newName = $"{baseName} ({suffix})";
                suffix++;
            } while (existingNames.Contains(newName));
            return newName;
        }
        else
        {
            // For default names, always use "Base 1", "Base 2", ...
            int suffix = 1;
            string newName;
            do
            {
                newName = $"{baseName} {suffix}";
                suffix++;
            } while (existingNames.Contains(newName));
            return newName;
        }
    }

    /// <summary>
    /// Optional runtime assignment
    /// </summary>
    public void Init(SaveListManager manager)
    {
        saveListManager = manager;
    }
}
