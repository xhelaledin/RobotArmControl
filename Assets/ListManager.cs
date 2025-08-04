using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ListManager : MonoBehaviour
{
    public GameObject saveItemPrefab;
    public Transform content;
    public RobotArmInputHandler4Parts armInputHandler4Parts;
    public RobotArmInputHandler5Parts armInputHandler5Parts;
    public RobotArmInputHandler5BParts armInputHandler5BParts;
    public RobotArmInputHandler6Parts armInputHandler6Parts;

    public BluetoothCommandConstructor bluetoothCommandConstructor;

    private Button currentVisibleButton2;
    private int selectedModelIndex;

    void Start()
    {
        // Get the selected model index from PlayerPrefs
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        PopulateList();
    }

    public void PopulateList()
    {

        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        
        // Destroy old items
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Create new save items for the selected model
        foreach (string name in GetAllSaveNames())
        {
            CreateSaveItem(name);
        }
    }

    private HashSet<string> GetAllSaveNames()
    {
        string raw = PlayerPrefs.GetString($"SaveList_{selectedModelIndex}", "");
        return new HashSet<string>(raw.Split(',').Where(n => !string.IsNullOrWhiteSpace(n)));
    }

    private void CreateSaveItem(string saveName)
    {
        GameObject newItem = Instantiate(saveItemPrefab, content);
        TMP_Text title = newItem.transform.Find("PositionTitle").GetComponent<TMP_Text>();
        title.text = saveName;

        Button deleteBtn = newItem.transform.Find("DeleteButton").GetComponent<Button>();
        Button runBtn = newItem.transform.Find("RunButton").GetComponent<Button>();
        Button viewBtn = newItem.transform.Find("SaveButton").GetComponent<Button>();
        Button runBtn2 = newItem.transform.Find("RunButton2").GetComponent<Button>();
        Button viewBtn2 = newItem.transform.Find("SaveButton2").GetComponent<Button>();

        runBtn2.gameObject.SetActive(false);
        viewBtn2.gameObject.SetActive(false);

        string localName = saveName;

        deleteBtn.onClick.AddListener(() => DeleteSave(localName, newItem));
        runBtn.onClick.AddListener(() => RunSave(localName, runBtn2));
        viewBtn.onClick.AddListener(() => ViewSave(localName, viewBtn2));
    }

    private void DeleteSave(string saveName, GameObject saveItem)
    {
        PlayerPrefs.DeleteKey($"SavedArray_{selectedModelIndex}_{saveName}");

        var names = GetAllSaveNames().ToList();
        names.Remove(saveName);
        PlayerPrefs.SetString($"SaveList_{selectedModelIndex}", string.Join(",", names));
        PlayerPrefs.Save();

        Destroy(saveItem);
    }

    private void RunSave(string saveName, Button runBtn2)
    {
        // Loading the saved data and applying it
        int[] values = LoadSave(saveName);
        ApplySavedValues(values);

        bluetoothCommandConstructor.ConstructSaveCommand(values);
    }

    private void ViewSave(string saveName, Button viewBtn2)
    {
        int[] values = LoadSave(saveName);
        ApplySavedValues(values);
    }

    private int[] LoadSave(string saveName)
    {
        string json = PlayerPrefs.GetString($"SavedArray_{selectedModelIndex}_{saveName}");
        ArmSaveData data = JsonUtility.FromJson<ArmSaveData>(json);
        return data.values;
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
