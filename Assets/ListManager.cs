using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ListManager : MonoBehaviour
{
    public GameObject saveItemPrefab;
    public Transform content;
    public RobotArmInputHandler armInput;
    public BluetoothManager bluetoothManager;

    // Store the previously pressed button to reset its state when a new button is pressed
    private Button currentVisibleButton2;

    void Start()
    {
        PopulateList();
    }

    private void PopulateList()
    {
        // Destroy old items
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Create new save items
        foreach (string name in GetAllSaveNames())
        {
            CreateSaveItem(name);
        }
    }

    private HashSet<string> GetAllSaveNames()
    {
        string raw = PlayerPrefs.GetString("SaveList", "");
        Debug.Log($"📄 Raw SaveList: {raw}");
        return new HashSet<string>(raw.Split(',').Where(n => !string.IsNullOrWhiteSpace(n)));
    }

    private void CreateSaveItem(string saveName)
    {
        Debug.Log($"📦 Creating Save Item: {saveName}");

        GameObject newItem = Instantiate(saveItemPrefab, content);
        TMP_Text title = newItem.transform.Find("PositionTitle").GetComponent<TMP_Text>();
        title.text = saveName;

        // Buttons in the prefab
        Button deleteBtn = newItem.transform.Find("DeleteButton").GetComponent<Button>();
        Button runBtn = newItem.transform.Find("RunButton").GetComponent<Button>();
        Button viewBtn = newItem.transform.Find("SaveButton").GetComponent<Button>(); // For View
        Button runBtn2 = newItem.transform.Find("RunButton2").GetComponent<Button>(); // Duplicate Run button (will appear when RunButton is pressed)
        Button viewBtn2 = newItem.transform.Find("SaveButton2").GetComponent<Button>(); // Duplicate View button (will appear when SaveButton is pressed)

        // Initially hide Button2s (the new buttons that will appear on top)
        runBtn2.gameObject.SetActive(false);
        viewBtn2.gameObject.SetActive(false);

        // Local name to avoid lambda closure issues
        string localName = saveName;

        // Add listeners for the buttons
        deleteBtn.onClick.AddListener(() => DeleteSave(localName, newItem));
        runBtn.onClick.AddListener(() => RunSave(localName, runBtn2));  // Pass RunButton2 to show it
        viewBtn.onClick.AddListener(() => ViewSave(localName, viewBtn2));  // Pass SaveButton2 to show it
    }

    private void DeleteSave(string saveName, GameObject saveItem)
    {
        Debug.Log($"🗑️ Deleting: {saveName}");

        PlayerPrefs.DeleteKey($"SavedArray_{saveName}");

        var names = GetAllSaveNames().ToList();
        names.Remove(saveName);
        PlayerPrefs.SetString("SaveList", string.Join(",", names));
        PlayerPrefs.Save();

        Destroy(saveItem);
    }

    private void ViewSave(string saveName, Button viewBtn2)
    {
        Debug.Log($"👁️ Viewing save: {saveName}");

        // Show Button2 when ViewButton is clicked
        viewBtn2.gameObject.SetActive(true);

        // Hide Button2 when any other button is pressed
        if (currentVisibleButton2 != null && currentVisibleButton2 != viewBtn2)
        {
            currentVisibleButton2.gameObject.SetActive(false);
        }

        // Update the currently visible Button2
        currentVisibleButton2 = viewBtn2;

        int[] values = LoadSavedValues(saveName);
        if (values == null) return;

        ApplyVisual(values);
    }

    private void RunSave(string saveName, Button runBtn2)
    {
        Debug.Log($"▶️ Running: {saveName}");

        // Show Button2 when RunButton is clicked
        runBtn2.gameObject.SetActive(true);

        // Hide Button2 when any other button is pressed
        if (currentVisibleButton2 != null && currentVisibleButton2 != runBtn2)
        {
            currentVisibleButton2.gameObject.SetActive(false);
        }

        // Update the currently visible Button2
        currentVisibleButton2 = runBtn2;

        int[] values = LoadSavedValues(saveName);
        if (values == null) return;

        string clawpos = "";
        if(values[3] == 0){
            clawpos = "105"; 
        }
        else {
            clawpos = "177";
        }
        

        string command = "SERVOS:" + values[0] + "," + values[1] + "," + values[2] + "," + clawpos;
        
        ApplyVisual(values);
        bluetoothManager.WriteData(command);
    }

    private int[] LoadSavedValues(string saveName)
    {
        string json = PlayerPrefs.GetString($"SavedArray_{saveName}", "");
        Debug.Log($"📦 Loaded JSON for {saveName}: {json}");

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning($"❌ No data found for \"{saveName}\"");
            return null;
        }

        ArmSaveData saveData = JsonUtility.FromJson<ArmSaveData>(json);
        if (saveData.values == null || saveData.values.Length < 4)
        {
            Debug.LogWarning($"❌ Invalid save data for \"{saveName}\"");
            return null;
        }

        Debug.Log($"📈 Loaded values: {string.Join(", ", saveData.values)}");
        return saveData.values;
    }

    private void ApplyVisual(int[] values)
    {
        Debug.Log($"🎮 Applying Visuals: {string.Join(", ", values)}");

        armInput.SetBaseRotation(values[0]);
        armInput.SetJoint1Rotation(values[1]);
        armInput.SetJoint2Rotation(values[2]);

        if (values[3] == 1)
        {
            armInput.SetJoints3Position();
        }
        else
        {
            armInput.ResetJoints3Position();
        }
    }

    private void SendBluetooth(int[] values)
    {
        Debug.Log($"📡 Sending Bluetooth Data: {string.Join(", ", values)}");

        bluetoothManager.dataToSend.text = "s1" + values[0];
        //bluetoothManager.WriteData();

        bluetoothManager.dataToSend.text = "s2" + values[1];
        //bluetoothManager.WriteData();

        bluetoothManager.dataToSend.text = "s3" + values[2];
        //bluetoothManager.WriteData();

        bluetoothManager.dataToSend.text = values[3] == 1 ? "s4Set" : "s4Reset";
        //bluetoothManager.WriteData();
    }
}
