using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ListManager : MonoBehaviour
{
    [Header("Save Items")]
    public GameObject saveItemPrefab;
    public GameObject noSavesPrefab;
    public Transform content;

    [Header("Robot Arm Handlers")]
    public RobotArmInputHandler4Parts armInputHandler4Parts;
    public RobotArmInputHandler5Parts armInputHandler5Parts;
    public RobotArmInputHandler5BParts armInputHandler5BParts;
    public RobotArmInputHandler6Parts armInputHandler6Parts;

    [Header("Bluetooth")]
    public BluetoothCommandConstructor bluetoothCommandConstructor;

    [Header("Button Sprites")]
    public Sprite runNormalSprite;
    public Sprite runSelectedSprite;
    public Sprite viewNormalSprite;
    public Sprite viewSelectedSprite;

    [Header("Add to List Panel")]
    public AddToListPanelManager addToListPanelManager; // Assign in inspector

    private int selectedModelIndex;
    private SaveItemManager currentlySelectedItem = null;
    private Button currentlySelectedButton = null;
    private string currentlySelectedType = "";

    private GameObject noSavesInstance = null;

    void Start()
    {
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        PopulateList();
    }

    public void PopulateList()
    {
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        foreach (Transform child in content)
        {
            if (noSavesInstance == null || child.gameObject != noSavesInstance)
                Destroy(child.gameObject);
        }

        HashSet<string> saves = GetAllSaveNames();

        if (saves.Count == 0)
        {
            if (noSavesPrefab != null && noSavesInstance == null)
            {
                noSavesInstance = Instantiate(noSavesPrefab, content);
            }
        }
        else
        {
            if (noSavesInstance != null)
            {
                Destroy(noSavesInstance);
                noSavesInstance = null;
            }

            foreach (string name in saves)
            {
                CreateSaveItem(name);
            }
        }

        currentlySelectedItem = null;
        currentlySelectedButton = null;
        currentlySelectedType = "";
    }

    private HashSet<string> GetAllSaveNames()
    {
        string raw = PlayerPrefs.GetString($"SaveList_{selectedModelIndex}", "");
        return new HashSet<string>(raw.Split(',').Where(n => !string.IsNullOrWhiteSpace(n)));
    }

    private void CreateSaveItem(string saveName)
    {
        GameObject newItem = Instantiate(saveItemPrefab, content);
        SaveItemManager saveItemManager = newItem.GetComponent<SaveItemManager>();

        string raw = PlayerPrefs.GetString($"SavedArray_{selectedModelIndex}_{saveName}");
        string[] parts = raw.Split(':');
        if (parts.Length >= 2)
        {
            string[] valueAndDate = parts[1].Split(';');
            if (valueAndDate.Length >= 2)
            {
                string valuesPart = valueAndDate[0];
                string datePart = valueAndDate[1];

                int[] values = valuesPart
                    .Split(',')
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToArray();

                saveItemManager.SetData(saveName, values, datePart);
            }
            else
            {
                saveItemManager.SetData(saveName, new int[0], "");
            }
        }
        else
        {
            saveItemManager.SetData(saveName, new int[0], "");
        }

        // Assign sprites
        saveItemManager.runNormalSprite = runNormalSprite;
        saveItemManager.runSelectedSprite = runSelectedSprite;
        saveItemManager.viewNormalSprite = viewNormalSprite;
        saveItemManager.viewSelectedSprite = viewSelectedSprite;

        // Setup buttons, including AddToList integration
        saveItemManager.SetupButtons(
            saveName,
            DeleteSave,
            OnRunButtonClicked,
            OnViewButtonClicked,
            AddSaveToListPanel
        );
    }

    private void DeleteSave(string saveName, GameObject saveItem)
    {
        PlayerPrefs.DeleteKey($"SavedArray_{selectedModelIndex}_{saveName}");

        var names = GetAllSaveNames().ToList();
        names.Remove(saveName);
        PlayerPrefs.SetString($"SaveList_{selectedModelIndex}", string.Join(",", names));
        PlayerPrefs.Save();

        Destroy(saveItem);

        if (currentlySelectedItem != null && currentlySelectedItem.gameObject == saveItem)
        {
            currentlySelectedItem = null;
            currentlySelectedButton = null;
            currentlySelectedType = "";
        }

        PopulateList();
    }

    private void OnRunButtonClicked(string saveName, Button runBtn)
    {
        int[] values = LoadSave(saveName);
        ApplySavedValuesExternal(values, true);

        SaveItemManager itemManager = runBtn.GetComponentInParent<SaveItemManager>();
        if (itemManager != null)
        {
            UpdateGlobalButtonVisuals(itemManager, runBtn, "run");
        }
    }

    private void OnViewButtonClicked(string saveName, Button viewBtn)
    {
        int[] values = LoadSave(saveName);
        ApplySavedValuesExternal(values, false); // runCommand = false

        SaveItemManager itemManager = viewBtn.GetComponentInParent<SaveItemManager>();
        if (itemManager != null)
        {
            UpdateGlobalButtonVisuals(itemManager, viewBtn, "view");
        }
    }

    private void UpdateGlobalButtonVisuals(SaveItemManager itemManager, Button clickedButton, string buttonType)
    {
        if (currentlySelectedButton != null && currentlySelectedItem != null)
        {
            if (currentlySelectedType == "run")
                currentlySelectedItem.SetRunButtonNormal();
            else if (currentlySelectedType == "view")
                currentlySelectedItem.SetViewButtonNormal();
        }

        if (buttonType == "run")
            itemManager.SetRunButtonSelected();
        else if (buttonType == "view")
            itemManager.SetViewButtonSelected();

        currentlySelectedButton = clickedButton;
        currentlySelectedType = buttonType;
        currentlySelectedItem = itemManager;
    }

    private int[] LoadSave(string saveName)
    {
        string raw = PlayerPrefs.GetString($"SavedArray_{selectedModelIndex}_{saveName}");
        if (string.IsNullOrEmpty(raw) || !raw.Contains(":"))
            return new int[0];

        string[] parts = raw.Split(':');
        if (parts.Length < 2)
            return new int[0];

        string[] valueAndDate = parts[1].Split(';');
        if (valueAndDate.Length < 1)
            return new int[0];

        return valueAndDate[0]
            .Split(',')
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToArray();
    }

    private void ApplySavedValues(int[] values)
    {
        switch (selectedModelIndex)
        {
            case 0:
                armInputHandler4Parts.ApplySavedValues(values);
                break;
            case 1:
                armInputHandler5Parts.ApplySavedValues(values);
                break;
            case 2:
                armInputHandler5BParts.ApplySavedValues(values);
                break;
            case 3:
                armInputHandler6Parts.ApplySavedValues(values);
                break;
        }
    }

    // External access for SaveListManager
    public int[] LoadSaveExternal(string saveName)
    {
        return LoadSave(saveName);
    }

    public void ApplySavedValuesExternal(int[] values, bool runCommand)
    {
        // ✅ Always apply the saved values to the robot arm
        ApplySavedValues(values);

        // Only send Bluetooth command if runCommand = true
        if (runCommand && bluetoothCommandConstructor != null)
        {
            bluetoothCommandConstructor.ConstructSaveCommand(values);
        }
    }

    // ✅ Open AddToListPanel for this save
    private void AddSaveToListPanel(string saveName)
    {
        if (addToListPanelManager != null)
        {
            addToListPanelManager.Open(saveName);
        }
        else
        {
            Debug.LogWarning("AddToListPanelManager not assigned in ListManager!");
        }
    }
}
