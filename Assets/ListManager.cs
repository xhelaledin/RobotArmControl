using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ListManager : MonoBehaviour
{
    public GameObject saveItemPrefab;
    public Transform content;

    public RobotArmInputHandler4Parts armInputHandler4Parts;
    public RobotArmInputHandler5Parts armInputHandler5Parts;
    public RobotArmInputHandler5BParts armInputHandler5BParts;
    public RobotArmInputHandler6Parts armInputHandler6Parts;

    public BluetoothCommandConstructor bluetoothCommandConstructor;

    [Header("Button Sprites")]
    public Sprite runNormalSprite;
    public Sprite runSelectedSprite;
    public Sprite viewNormalSprite;
    public Sprite viewSelectedSprite;

    private int selectedModelIndex;

    // Track global selection across all save items
    private SaveItemManager currentlySelectedItem = null;
    private Button currentlySelectedButton = null;
    private string currentlySelectedType = "";

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
            Destroy(child.gameObject);
        }

        foreach (string name in GetAllSaveNames())
        {
            CreateSaveItem(name);
        }

        // Reset selection when list is refreshed
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
                // fallback if no date
                saveItemManager.SetData(saveName, new int[0], "");
            }
        }
        else
        {
            saveItemManager.SetData(saveName, new int[0], "");
        }

        // Assign sprites for buttons
        saveItemManager.runNormalSprite = runNormalSprite;
        saveItemManager.runSelectedSprite = runSelectedSprite;
        saveItemManager.viewNormalSprite = viewNormalSprite;
        saveItemManager.viewSelectedSprite = viewSelectedSprite;

        saveItemManager.SetupButtons(
            saveName,
            DeleteSave,
            OnRunButtonClicked,
            OnViewButtonClicked
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

        // If the deleted item was the selected one, clear selection
        if (currentlySelectedItem != null && currentlySelectedItem.gameObject == saveItem)
        {
            currentlySelectedItem = null;
            currentlySelectedButton = null;
            currentlySelectedType = "";
        }
    }

    private void OnRunButtonClicked(string saveName, Button runBtn)
    {
        int[] values = LoadSave(saveName);
        ApplySavedValues(values);
        bluetoothCommandConstructor.ConstructSaveCommand(values);

        SaveItemManager itemManager = runBtn.GetComponentInParent<SaveItemManager>();
        if (itemManager != null)
        {
            UpdateGlobalButtonVisuals(itemManager, runBtn, "run");
        }
    }

    private void OnViewButtonClicked(string saveName, Button viewBtn)
    {
        int[] values = LoadSave(saveName);
        ApplySavedValues(values);

        SaveItemManager itemManager = viewBtn.GetComponentInParent<SaveItemManager>();
        if (itemManager != null)
        {
            UpdateGlobalButtonVisuals(itemManager, viewBtn, "view");
        }
    }

    private void UpdateGlobalButtonVisuals(SaveItemManager itemManager, Button clickedButton, string buttonType)
    {
        // Reset previous selected button sprite if any
        if (currentlySelectedButton != null && currentlySelectedItem != null)
        {
            if (currentlySelectedType == "run")
                currentlySelectedItem.SetRunButtonNormal();
            else if (currentlySelectedType == "view")
                currentlySelectedItem.SetViewButtonNormal();
        }

        // Set newly clicked button sprite
        if (buttonType == "run")
            itemManager.SetRunButtonSelected();
        else if (buttonType == "view")
            itemManager.SetViewButtonSelected();

        // Update tracking
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
}
