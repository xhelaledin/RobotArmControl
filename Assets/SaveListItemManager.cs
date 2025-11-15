using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

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

    private Coroutine _rebuildCoroutine;

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
        
        RequestRebuild();
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
            
            entry.Setup(saveRef, i, this, saveListManager, listManager);
        }
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
        RequestRebuild();
    }

    public void RemoveEntry(int index)
    {
        if (index < 0 || index >= currentListData.saves.Count) return;

        currentListData.saves.RemoveAt(index);
        saveListManager.SaveLists();

        savesCountText.text = $"{currentListData.saves.Count} saves";

        if (currentListData.saves.Count == 0 && expanded)
        {
            ToggleExpand();
        }
        else if (expanded)
        {
            RefreshEntries();
            RequestRebuild();
        }
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

        saveListManager.StopActiveCoroutine();

        var save = currentListData.saves[index];
        listManager.ApplySavedValuesExternal(save.values.ToArray(), false);
    }

    private void RequestRebuild()
    {
        if (_rebuildCoroutine != null)
        {
            StopCoroutine(_rebuildCoroutine);
        }
        
        if (gameObject.activeInHierarchy)
        {
            _rebuildCoroutine = StartCoroutine(RebuildLayoutAtEndOfFrame());
        }
    }

    private IEnumerator RebuildLayoutAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();


        if (entriesContainer != null && entriesContainer.gameObject.activeInHierarchy)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(entriesContainer.GetComponent<RectTransform>());
        }

        if (this != null && gameObject.activeInHierarchy)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        if (parentListContainer != null && parentListContainer.gameObject.activeInHierarchy)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentListContainer.GetComponent<RectTransform>());
        }

        _rebuildCoroutine = null;
    }
}