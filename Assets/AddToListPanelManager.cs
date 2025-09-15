using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AddToListPanelManager : MonoBehaviour, IHideablePanel, IHideablePanel2
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

    // --- Main Panel (IHideablePanel) ---
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
        PanelManager.Instance.RegisterPanel(this);
    }

    public void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public bool IsPanelActive()
    {
        return panel != null && panel.activeSelf;
    }

    // --- Rename Popup (IHideablePanel2) ---
    private void OpenRenamePopup()
    {
        if (renamePopup == null || renameInputField == null) return;

        renameInputField.text = "";
        renamePopup.SetActive(true);

        PanelManager.Instance?.PushPanel(
            key: renamePopup,
            hide: HidePanel2,
            isActive: IsPanelActive2
        );
        PanelManager.Instance.RegisterPanel2(this);
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
        saveListManager.AddSaveToList(saveToAdd, listName, allowDuplicates: true);

        GameObject item = Instantiate(listItemPrefab, listContent);
        item.transform.SetSiblingIndex(1);
        AddToListItemManager manager = item.GetComponent<AddToListItemManager>();
        manager.SetData(listName);
        manager.Select();
        toggleItems.Insert(1, manager);

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
