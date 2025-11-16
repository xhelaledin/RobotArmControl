using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class SaveListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveItemPrefab;
    public Transform listContent;
    public GameObject listPanel;
    public Button closePanelButton;

    [Header("Empty / Create UI")]
    public GameObject noListsPrefab;
    public Button createListButton;

    [Header("Dependencies")]
    public ListManager listManager;
    public AddToListPanelManager addToListPanelManager;

    private Dictionary<int, Dictionary<string, SaveListData>> saveListsGrouped = new();
    private int currentModelIndex;
    private const string SaveListsGroupedKey = "SaveListsGrouped";

    private Coroutine activeCoroutine;
    private string activeListName;
    private bool activeRunCommand;

    private void Start()
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(() => listPanel.SetActive(false));

        if (createListButton != null)
            createListButton.onClick.AddListener(() =>
            {
                if (addToListPanelManager != null)
                {
                    addToListPanelManager.OpenRenamePopupExternal();
                }
                else
                {
                    Debug.LogWarning("SaveListManager: AddToListPanelManager not assigned!");
                }
            });

        LoadLists();
        RefreshUI();
    }

    public void LoadFromPlayerPrefs()
    {
        LoadLists();
        RefreshUI();
    }

    private void LoadLists()
    {
        string raw = PlayerPrefs.GetString(SaveListsGroupedKey, "");
        if (!string.IsNullOrEmpty(raw))
        {
            try
            {
                var grouped = JsonUtilityWrapper.FromJsonGrouped(raw)?.ToGroupedDictionary();
                if (grouped != null)
                    saveListsGrouped = grouped;
            }
            catch
            {
                saveListsGrouped = new();
            }
        }

        for (int i = 0; i <= 3; i++)
            if (!saveListsGrouped.ContainsKey(i))
                saveListsGrouped[i] = new();
    }

    public void SaveLists()
    {
        string json = JsonUtilityWrapper.ToJson(new GroupedSerializableDictionary(saveListsGrouped));
        PlayerPrefs.SetString(SaveListsGroupedKey, json);
        PlayerPrefs.Save();
    }

    public void ShowPanel()
    {
        StopActiveCoroutine();

        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        listPanel.SetActive(true);
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        var activeLists = saveListsGrouped[currentModelIndex];

        if (activeLists.Count == 0)
        {
            if (noListsPrefab != null)
                Instantiate(noListsPrefab, listContent);
        }
        else
        {
            foreach (var kvp in activeLists)
            {
                GameObject item = Instantiate(saveItemPrefab, listContent);
                SaveListItemManager manager = item.GetComponent<SaveListItemManager>();

                manager.parentListContainer = listContent;
                manager.SetData(
                    kvp.Key,
                    kvp.Value,
                    (name) => ToggleRunList(name, true),
                    (name) => ToggleRunList(name, false),
                    DeleteList,
                    listManager,
                    this
                );
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(listContent.GetComponent<RectTransform>());
    }

    // --- Coroutine Handling ---
    private void ToggleRunList(string listName, bool runCommand)
    {
        if (activeCoroutine != null && activeListName == listName && activeRunCommand == runCommand)
        {
            StopActiveCoroutine();
            return;
        }

        StopActiveCoroutine();

        activeListName = listName;
        activeRunCommand = runCommand;
        activeCoroutine = StartCoroutine(RunSavesCoroutine(listName, 1f, runCommand));
    }

    private IEnumerator RunSavesCoroutine(string listName, float delay, bool runCommand)
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        if (!saveListsGrouped[currentModelIndex].ContainsKey(listName)) yield break;

        var saves = saveListsGrouped[currentModelIndex][listName].saves;

        foreach (var save in saves)
        {
            listManager.ApplySavedValuesExternal(save.values.ToArray(), runCommand);
            yield return new WaitForSeconds(save.delayMs / 1000f);
        }

        activeCoroutine = null;
        activeListName = null;
    }

    public void StopActiveCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
            activeListName = null;
        }
    }

    public void CreateList(string listName)
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        var activeLists = saveListsGrouped[currentModelIndex];

        if (!activeLists.ContainsKey(listName))
        {
            activeLists[listName] = new SaveListData
            {
                saves = new(),
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };
            SaveLists();
            RefreshUI();
        }
    }

    public void AddSaveToList(string saveName, string listName, bool allowDuplicates = false)
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        var activeLists = saveListsGrouped[currentModelIndex];

        if (!activeLists.ContainsKey(listName))
            CreateList(listName);

        string saveKey = $"SavedArray_{currentModelIndex}_{saveName}";
        string raw = PlayerPrefs.GetString(saveKey, null);
        if (string.IsNullOrEmpty(raw)) return;

        int colonIndex = raw.IndexOf(':');
        int semicolonIndex = raw.IndexOf(';');
        if (colonIndex < 0 || semicolonIndex < 0) return;

        string valuesStr = raw[(colonIndex + 1)..semicolonIndex];
        string dateStr = raw[(semicolonIndex + 1)..];
        var values = valuesStr.Split(',').Select(int.Parse).ToList();

        if (!allowDuplicates && activeLists[listName].saves.Any(s => s.saveName == saveName))
            return;

        activeLists[listName].saves.Add(new SaveReference
        {
            saveName = saveName,
            values = values,
            dateString = dateStr,
            delayMs = 1000
        });

        SaveLists();
        RefreshUI();
    }

    public Dictionary<string, SaveListData> GetAllListsForCurrentModel()
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        if (!saveListsGrouped.ContainsKey(currentModelIndex))
            saveListsGrouped[currentModelIndex] = new();
        return saveListsGrouped[currentModelIndex];
    }

    private void DeleteList(string listName)
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        var activeLists = saveListsGrouped[currentModelIndex];
        if (activeLists.ContainsKey(listName))
        {
            activeLists.Remove(listName);
            SaveLists();
            RefreshUI();
        }
    }

    public Dictionary<int, Dictionary<string, SaveListData>> GetGroupedLists()
    {
        for (int i = 0; i <= 3; i++)
            if (!saveListsGrouped.ContainsKey(i))
                saveListsGrouped[i] = new();
        return saveListsGrouped;
    }
}
