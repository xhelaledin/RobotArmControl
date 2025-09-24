using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveListItemManager : MonoBehaviour
{
    [Header("Header UI")]
    public TMP_Text listNameText;
    public TMP_Text savesCountText;
    public TMP_Text dateText;

    public Button runButton;
    public Button viewButton;
    public Button deleteButton;
    public Button expandButton;

    [Header("Entries UI")]
    public Transform entriesContainer;
    public GameObject entryPrefab;

    [HideInInspector]
    public Transform parentListContainer;

    private string currentListName;
    private SaveListData currentListData;
    private SaveListManager saveListManager;
    private ListManager listManager;
    private bool expanded = false;

    public void SetData(string listName, SaveListData listData,
        Action<string> runAction,
        Action<string> viewAction,
        Action<string> deleteAction,
        ListManager listMgr,
        SaveListManager saveListMgr)
    {
        currentListName = listName;
        currentListData = listData;
        listManager = listMgr;
        saveListManager = saveListMgr;

        listNameText.text = listName;
        savesCountText.text = $"{listData.saves.Count} saves";
        dateText.text = listData.createdDate;

        runButton.onClick.RemoveAllListeners();
        viewButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        expandButton.onClick.RemoveAllListeners();

        runButton.onClick.AddListener(() => runAction?.Invoke(currentListName));
        viewButton.onClick.AddListener(() => viewAction?.Invoke(currentListName));
        deleteButton.onClick.AddListener(() => deleteAction?.Invoke(currentListName));
        expandButton.onClick.AddListener(ToggleExpand);

        entriesContainer.gameObject.SetActive(false);
    }

    private void ToggleExpand()
    {
        expanded = !expanded;
        entriesContainer.gameObject.SetActive(expanded);

        if (expanded)
            RefreshEntries();

        ForceRebuild();
    }

    private void RefreshEntries()
    {
        foreach (Transform child in entriesContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < currentListData.saves.Count; i++)
        {
            var saveRef = currentListData.saves[i];
            GameObject entryGO = Instantiate(entryPrefab, entriesContainer);
            SaveListEntryManager entry = entryGO.GetComponent<SaveListEntryManager>();
            
            // ✅ Pass the required arguments now
            entry.Setup(saveRef, i, this, saveListManager, listManager);
        }

        ForceRebuild();
    }

    public void MoveEntry(int index, int direction)
    {
        int newIndex = index + direction;
        if (newIndex < 0 || newIndex >= currentListData.saves.Count) return;

        var item = currentListData.saves[index];
        currentListData.saves.RemoveAt(index);
        currentListData.saves.Insert(newIndex, item);

        saveListManager.SaveLists();
        RefreshEntries();
    }

    public void RemoveEntry(int index)
    {
        if (index < 0 || index >= currentListData.saves.Count) return;

        currentListData.saves.RemoveAt(index);
        saveListManager.SaveLists();

        RefreshEntries();
        savesCountText.text = $"{currentListData.saves.Count} saves";
        ForceRebuild();

        ToggleExpand();
    }

    public void UpdateDelay(int index, int newDelayMs)
    {
        if (index < 0 || index >= currentListData.saves.Count) return;
        currentListData.saves[index].delayMs = Mathf.Max(0, newDelayMs);
        saveListManager.SaveLists();
    }

    public void ViewEntry(int index)
    {
        if (index < 0 || index >= currentListData.saves.Count) return;

        // Stop coroutine before showing a single entry
        saveListManager.StopActiveCoroutine();

        var save = currentListData.saves[index];
        listManager.ApplySavedValuesExternal(save.values.ToArray(), false);
    }

    private void ForceRebuild()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(entriesContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        if (parentListContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentListContainer.GetComponent<RectTransform>());
    }
}
