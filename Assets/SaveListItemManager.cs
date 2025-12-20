using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

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

    public Transform expandIconTransform;

    [Header("Sprites")]
    public Sprite runNormalSprite;
    public Sprite runSelectedSprite;
    public Sprite viewNormalSprite;   
    public Sprite viewSelectedSprite; 

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
    
    private List<SaveListEntryManager> spawnedEntries = new List<SaveListEntryManager>();

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

        // These actions trigger HandleListAction in SaveListManager
        runButton.onClick.AddListener(() => runAction?.Invoke(currentListName));
        viewButton.onClick.AddListener(() => viewAction?.Invoke(currentListName));
        deleteButton.onClick.AddListener(() => deleteAction?.Invoke(currentListName));
        expandButton.onClick.AddListener(ToggleExpand);

        entriesContainer.gameObject.SetActive(false);
        
        // Ensure default state
        SetRunButtonVisual(false);
        SetViewButtonVisual(false);
    }

    private void ToggleExpand()
    {
        expanded = !expanded;
        entriesContainer.gameObject.SetActive(expanded);

        if (expandIconTransform != null)
        {
            float targetZRotation = expanded ? 180f : 0f;
            expandIconTransform.eulerAngles = new Vector3(0, 0, targetZRotation);
        }

        if (expanded)
            RefreshEntries();
        
        RequestRebuild();
    }

    private void RefreshEntries()
    {
        foreach (Transform child in entriesContainer)
            Destroy(child.gameObject);

        spawnedEntries.Clear();

        for (int i = 0; i < currentListData.saves.Count; i++)
        {
            var saveRef = currentListData.saves[i];
            GameObject entryGO = Instantiate(entryPrefab, entriesContainer);
            SaveListEntryManager entry = entryGO.GetComponent<SaveListEntryManager>();
            
            entry.Setup(saveRef, i, this, saveListManager, listManager);
            spawnedEntries.Add(entry);
        }
    }

    // --- Visual State Methods ---

    public void SetRunButtonVisual(bool isActive)
    {
        if (runButton != null)
        {
            Image img = runButton.GetComponent<Image>();
            if (img != null && runNormalSprite != null && runSelectedSprite != null)
            {
                img.sprite = isActive ? runSelectedSprite : runNormalSprite;
            }
        }
    }

    public void SetViewButtonVisual(bool isActive)
    {
        if (viewButton != null)
        {
            Image img = viewButton.GetComponent<Image>();
            if (img != null && viewNormalSprite != null && viewSelectedSprite != null)
            {
                img.sprite = isActive ? viewSelectedSprite : viewNormalSprite;
            }
        }
    }

    public void HighlightEntryVisual(int index)
    {
        if (!expanded) return;

        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (spawnedEntries[i] != null)
            {
                // Highlights the view button of the specific entry
                spawnedEntries[i].SetViewButtonVisual(i == index);
            }
        }
    }

    public void ResetAllEntriesVisuals()
    {
        foreach (var entry in spawnedEntries)
        {
            if (entry != null)
                entry.SetViewButtonVisual(false);
        }
    }

    // ----------------------------

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

        // Stop any running sequence
        saveListManager.StopActiveCoroutine();
        
        // Reset all visuals (headers and items)
        saveListManager.ResetAllVisuals(); 
        
        // --- CHANGE: We removed SetViewButtonVisual(true) here ---
        // The parent header button will now remain unpressed.

        // Highlight this specific entry
        HighlightEntryVisual(index);

        var save = currentListData.saves[index];
        // Apply values: False = No Bluetooth (View only)
        listManager.ApplySavedValuesExternal(save.values.ToArray(), false);
    }

    private void RequestRebuild()
    {
        if (_rebuildCoroutine != null) StopCoroutine(_rebuildCoroutine);
        if (gameObject.activeInHierarchy) _rebuildCoroutine = StartCoroutine(RebuildLayoutAtEndOfFrame());
    }

    private IEnumerator RebuildLayoutAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        if (entriesContainer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(entriesContainer.GetComponent<RectTransform>());
        if (this != null) LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        if (parentListContainer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(parentListContainer.GetComponent<RectTransform>());
        _rebuildCoroutine = null;
    }
}