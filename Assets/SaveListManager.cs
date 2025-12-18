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
    
    // Track active UI managers to update visuals during runtime
    private Dictionary<string, SaveListItemManager> activeItemManagers = new Dictionary<string, SaveListItemManager>();

    private void Awake()
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
    }

    private void Start()
    {
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
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        listPanel.SetActive(true);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (saveListsGrouped == null || !saveListsGrouped.ContainsKey(0))
            LoadLists();

        foreach (Transform child in listContent)
            Destroy(child.gameObject);
        
        activeItemManagers.Clear();

        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        if (!saveListsGrouped.ContainsKey(currentModelIndex))
            saveListsGrouped[currentModelIndex] = new Dictionary<string, SaveListData>();

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
                    (name) => HandleListAction(name, true),  // True = Send Bluetooth (Run)
                    (name) => HandleListAction(name, false), // False = No Bluetooth (View)
                    DeleteList,
                    listManager,
                    this
                );

                activeItemManagers[kvp.Key] = manager;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(listContent.GetComponent<RectTransform>());
    }

    // --- Action Handling ---

    private void HandleListAction(string listName, bool isRunAction)
    {
        // 1. Stop any existing coroutine
        StopActiveCoroutine();

        // 2. Reset visuals of ALL lists (including the current one briefly, to clean slate)
        foreach (var kvp in activeItemManagers)
        {
            kvp.Value.SetRunButtonVisual(false);
            kvp.Value.SetViewButtonVisual(false);
            kvp.Value.ResetAllEntriesVisuals();
        }

        // 3. Get current manager
        if (!activeItemManagers.ContainsKey(listName)) return;
        SaveListItemManager uiManager = activeItemManagers[listName];

        // 4. Set visual state based on which button was clicked
        // Run Button clicked -> Run=Active, View=Inactive
        // View Button clicked -> Run=Inactive, View=Active
        uiManager.SetRunButtonVisual(isRunAction);
        uiManager.SetViewButtonVisual(!isRunAction);

        // 5. Start Sequence
        // We pass 'isRunAction' as the 'sendBluetooth' flag.
        // True = Run (Visuals + Bluetooth)
        // False = View (Visuals only)
        activeListName = listName;
        activeCoroutine = StartCoroutine(RunSavesCoroutine(listName, isRunAction));
    }

    private IEnumerator RunSavesCoroutine(string listName, bool sendBluetooth)
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        if (!saveListsGrouped[currentModelIndex].ContainsKey(listName)) yield break;

        SaveListItemManager uiManager = null;
        if (activeItemManagers.ContainsKey(listName))
            uiManager = activeItemManagers[listName];

        var saves = saveListsGrouped[currentModelIndex][listName].saves;

        for (int i = 0; i < saves.Count; i++)
        {
            var save = saves[i];

            // Highlight the specific entry being shown
            if (uiManager != null)
                uiManager.HighlightEntryVisual(i);

            // Apply values. 
            // If sendBluetooth is true, it moves model + sends commands.
            // If sendBluetooth is false, it only moves model.
            listManager.ApplySavedValuesExternal(save.values.ToArray(), sendBluetooth); 
            
            yield return new WaitForSeconds(save.delayMs / 1000f);
        }

        // Coroutine finished naturally. 
        // We leave the buttons "pressed" and the last entry highlighted so the user knows where it stopped.
        
        activeCoroutine = null;
        activeListName = null;
    }

    public void StopActiveCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        activeListName = null;
    }

    // Call this from RobotArmSelection when resetting/moving manually to clean up UI
    public void ResetAllVisuals()
    {
        StopActiveCoroutine();

        foreach (var manager in activeItemManagers.Values)
        {
            if (manager != null)
            {
                manager.SetRunButtonVisual(false);
                manager.SetViewButtonVisual(false);
                manager.ResetAllEntriesVisuals();
            }
        }
    }

    public void CreateList(string listName)
    {
        currentModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        
        if (!saveListsGrouped.ContainsKey(currentModelIndex)) 
            saveListsGrouped[currentModelIndex] = new Dictionary<string, SaveListData>();

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
        
        if (!saveListsGrouped.ContainsKey(currentModelIndex)) 
            saveListsGrouped[currentModelIndex] = new Dictionary<string, SaveListData>();

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