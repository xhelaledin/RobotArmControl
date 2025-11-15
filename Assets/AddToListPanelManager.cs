using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AddToListPanelManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Button closeButton;
    public Button cancelButton;
    public Button confirmButton;
    public GameObject listItemPrefab;
    public Transform listContent;

    [Header("Create New Item Prefab")]
    public GameObject createNewItemPrefab;

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

        closeButton?.onClick.AddListener(() => HidePanel());
        cancelButton?.onClick.AddListener(() => HidePanel());
        confirmButton?.onClick.AddListener(AddToSelectedLists);

        renameCancelButton?.onClick.AddListener(() => HidePanel2());
        renameConfirmButton?.onClick.AddListener(ConfirmCreateNewList);
    }

    // --- Main Panel ---
    public void Open(string saveName)
    {
        saveToAdd = saveName;
        panel.SetActive(true);
        RefreshList();

        PanelManager.Instance?.PushPanel(
            key: panel,
            hide: HidePanel,
            isActive: IsPanelActive
        );
    }

    public void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public bool IsPanelActive()
    {
        return panel != null && panel.activeSelf;
    }

    // --- Rename Popup ---
    public void OpenRenamePopup()
    {
        if (renamePopup == null || renameInputField == null) return;

        renameInputField.text = "";
        renamePopup.SetActive(true);

        PanelManager.Instance?.PushPanel(
            key: renamePopup,
            hide: HidePanel2,
            isActive: IsPanelActive2
        );
    }

    // ✅ Used by SaveListManager when pressing "Create New List"
    public void OpenRenamePopupExternal()
    {
        saveToAdd = null; // don’t add a save, just create empty list
        OpenRenamePopup();
    }

    public void HidePanel2()
    {
        if (renamePopup != null) renamePopup.SetActive(false);
    }

    public bool IsPanelActive2()
    {
        return renamePopup != null && renamePopup.activeSelf;
    }

    // --- Refresh List ---
    private void RefreshList()
    {
        if (saveListManager == null || listItemPrefab == null || createNewItemPrefab == null) return;
        toggleItems.Clear();

        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        var lists = saveListManager.GetAllListsForCurrentModel();
        if (lists == null) return;

        GameObject createNewGO = Instantiate(createNewItemPrefab, listContent);
        AddToListItemManager createManager = createNewGO.GetComponent<AddToListItemManager>();
        createManager.SetAsCreateNew(OpenRenamePopup);
        toggleItems.Add(createManager);

        foreach (var kvp in lists)
        {
            GameObject item = Instantiate(listItemPrefab, listContent);
            AddToListItemManager manager = item.GetComponent<AddToListItemManager>();
            manager.SetData(kvp.Key);
            toggleItems.Add(manager);
        }
    }

    // --- Confirm Actions ---
    private void AddToSelectedLists()
    {
        foreach (var item in toggleItems)
        {
            if (item.IsSelected && !item.IsCreateNew)
                saveListManager.AddSaveToList(saveToAdd, item.ListName, allowDuplicates: true);
        }
        HidePanel();
    }

    private void ConfirmCreateNewList()
    {
        if (renameInputField == null || saveListManager == null || listItemPrefab == null) return;

        string baseName = renameInputField.text.Trim();
        bool isDefault = string.IsNullOrEmpty(baseName);
        if (isDefault) baseName = defaultListName;

        HashSet<string> existingLists = new(saveListManager.GetAllListsForCurrentModel().Keys);
        string listName = GenerateUniqueName(baseName, existingLists, isDefault);

        saveListManager.CreateList(listName);

        // ✅ Only add save if we opened with a saveName
        if (!string.IsNullOrEmpty(saveToAdd))
            saveListManager.AddSaveToList(saveToAdd, listName, allowDuplicates: true);

        HidePanel2();
        HidePanel();
    }

    // --- Utils ---
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