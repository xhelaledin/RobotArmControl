using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[Serializable]
public class SaveListData
{
    public List<string> saves = new List<string>();
    public string createdDate;
}

public class SaveListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveItemPrefab;       // Prefab for individual saves (SaveItemManager)
    public Transform listContent;           // Scroll content
    public GameObject listPanel;            // The SavedListsPanel root
    public Button closePanelButton;

    [Header("Dependencies")]
    public ListManager listManager;         // Reference to existing ListManager

    private Dictionary<string, SaveListData> saveLists = new Dictionary<string, SaveListData>();

    private void Start()
    {
        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(() => listPanel.SetActive(false));

        LoadLists();
        RefreshUI();
    }

    private void LoadLists()
    {
        string raw = PlayerPrefs.GetString("SaveLists", "");
        if (!string.IsNullOrEmpty(raw))
        {
            saveLists = JsonUtilityWrapper.FromJson<SerializableDictionary>(raw).ToDictionary();
        }
    }

    private void SaveLists()
    {
        string json = JsonUtilityWrapper.ToJson(new SerializableDictionary(saveLists));
        PlayerPrefs.SetString("SaveLists", json);
        PlayerPrefs.Save();
    }

    public void ShowPanel()
    {
        listPanel.SetActive(true);
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        foreach (var kvp in saveLists)
        {
            GameObject item = Instantiate(saveItemPrefab, listContent);
            SaveListItemManager manager = item.GetComponent<SaveListItemManager>();

            string listName = kvp.Key;

            // Wrap the method in a lambda to match Action<string>
            manager.SetData(
                listName,
                kvp.Value,
                (name) => RunListSequentially(name, 1f, true),   // run
                (name) => RunListSequentially(name, 1f, false),  // view
                DeleteList,
                listManager
            );
        }
    }


    /// <summary>
    /// Run or view saves of a specific list sequentially, with delay
    /// </summary>
    public void RunListSequentially(string listName, float delay = 1f, bool runCommand = true)
    {
        if (!saveLists.ContainsKey(listName)) return;
        StartCoroutine(RunSavesCoroutine(listName, delay, runCommand));
    }

    private IEnumerator RunSavesCoroutine(string listName, float delay, bool runCommand)
    {
        var saves = saveLists[listName].saves;
        Debug.Log($"[RunSavesCoroutine] Running {saves.Count} saves for list '{listName}'");

        foreach (string saveName in saves)
        {
            Debug.Log($"[RunSavesCoroutine] Applying save: {saveName}");

            int[] values = listManager.LoadSaveExternal(saveName);
            listManager.ApplySavedValuesExternal(values, runCommand);

            // Wait before applying the next save
            yield return new WaitForSeconds(delay);
        }

        Debug.Log("[RunSavesCoroutine] Finished running all saves");
    }

    public void CreateList(string listName)
    {
        if (!saveLists.ContainsKey(listName))
        {
            saveLists[listName] = new SaveListData()
            {
                saves = new List<string>(),
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };
            SaveLists();
            RefreshUI();
        }
    }

    public void AddSaveToList(string saveName, string listName, bool allowDuplicates = false)
    {
        if (!saveLists.ContainsKey(listName))
            CreateList(listName);

        if (allowDuplicates || !saveLists[listName].saves.Contains(saveName))
        {
            saveLists[listName].saves.Add(saveName);
            SaveLists();
            RefreshUI();
        }
    }

    public Dictionary<string, SaveListData> GetAllLists()
    {
        return saveLists;
    }

    private void DeleteList(string listName)
    {
        if (saveLists.ContainsKey(listName))
        {
            saveLists.Remove(listName);
            SaveLists();
            RefreshUI();
        }
    }
}
