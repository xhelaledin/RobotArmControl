using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using NativeFilePickerNamespace;  // Match your NativeFilePicker plugin namespace

[Serializable]
public class SaveEntry
{
    public string saveName;
    public List<int> values;
    public string dateString;

    public SaveEntry(string saveName, List<int> values, string dateString)
    {
        this.saveName = saveName;
        this.values = values;
        this.dateString = dateString;
    }
}

[Serializable]
public class SaveDataWrapper
{
    public int modelIndex;
    public List<SaveEntry> entries;

    public SaveDataWrapper(int modelIndex, List<SaveEntry> entries)
    {
        this.modelIndex = modelIndex;
        this.entries = entries;
    }
}

[Serializable]
public class AllModelsSaveData
{
    public List<SaveDataWrapper> allModelsSaves;

    public AllModelsSaveData(List<SaveDataWrapper> allModelsSaves)
    {
        this.allModelsSaves = allModelsSaves;
    }
}

public class SaveManager : MonoBehaviour, IHideablePanel
{
    public GameObject popupPanel;
    public TMP_InputField inputField;
    public Button popupSaveButton, popupCancelButton;
    public Slider[] sliders;

    public Button closeButton;    // ✅ "Close"
    public Button openButton;     // ✅ "Open"

    public Button exportAllButton; // New button for exporting all
    public Button importAllButton; // New button for importing all

    private string defaultNamePrefix = "Position";
    private int selectedModelIndex; // Model index

    private SaveDataWrapper lastImportedData;

    void Start()
    {
        popupPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseButtonPressed);
        if (openButton != null)
            openButton.onClick.AddListener(OpenButtonPressed);

        if (exportAllButton != null)
            exportAllButton.onClick.AddListener(ExportAllSavesToFile);
        if (importAllButton != null)
            importAllButton.onClick.AddListener(ImportAllSavesFromFile);

        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        // Debug.Log("Selected Model Index: " + selectedModelIndex);

        if (popupSaveButton != null)
            popupSaveButton.onClick.AddListener(StartSavingProcess);
        if (popupCancelButton != null)
            popupCancelButton.onClick.AddListener(HidePanel);
    }

    private string GetSaveKey(int modelIndex, string saveName)
    {
        return $"SavedArray_{modelIndex}_{saveName}";
    }

    private string GetSaveListKey(int modelIndex)
    {
        return $"SaveList_{modelIndex}";
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        inputField.text = "";

        PanelManager.Instance.RegisterPanel(this);
    }

    public void StartSavingProcess()
    {
        string baseName = inputField.text.Trim();
        string initialName = string.IsNullOrWhiteSpace(baseName) ? GenerateDefaultName() : baseName;

        HashSet<string> existingNames = GetAllSaveNames(selectedModelIndex);
        string saveName = GenerateUniqueName(initialName, existingNames);

        SaveArray(selectedModelIndex, saveName);

        Toast("Saved as: " + saveName);
        HidePanel();
    }

    private string GenerateUniqueName(string baseName, HashSet<string> existingNames)
    {
        if (!existingNames.Contains(baseName)) return baseName;

        int suffix = 1;
        string newName;
        do
        {
            newName = $"{baseName} ({suffix})";
            suffix++;
        } while (existingNames.Contains(newName));

        return newName;
    }

    private string GenerateDefaultName()
    {
        int count = 1;
        HashSet<string> names = GetAllSaveNames(selectedModelIndex);
        string name;

        do
        {
            name = $"{defaultNamePrefix} {count}";
            count++;
        } while (names.Contains(name));

        return name;
    }

    private HashSet<string> GetAllSaveNames(int modelIndex)
    {
        string raw = PlayerPrefs.GetString(GetSaveListKey(modelIndex), "");
        return new HashSet<string>(raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()));
    }

    private void SaveArray(int modelIndex, string saveName)
    {
        var names = GetAllSaveNames(modelIndex).ToList();

        if (!names.Contains(saveName))
        {
            names.Add(saveName);
            PlayerPrefs.SetString(GetSaveListKey(modelIndex), string.Join(",", names));
        }

        // Determine how many sliders to save per model
        int sliderCount = modelIndex switch
        {
            0 => 3,
            1 or 2 => 4,
            3 => 5,
            _ => 3
        };

        List<int> saveValues = new List<int>();

        for (int i = 0; i < sliderCount; i++)
            saveValues.Add(Mathf.RoundToInt(sliders[i].value));

        int openCloseState = PlayerPrefs.GetInt("OpenCloseButtonPressed", 0);
        saveValues.Add(openCloseState);

        string dateString = DateTime.Now.ToString("dddd, dd.MM.yyyy - HH·mm");

        string saveString = $"{saveName}:{string.Join(",", saveValues)};{dateString}";
        PlayerPrefs.SetString(GetSaveKey(modelIndex, saveName), saveString);
        PlayerPrefs.Save();

        Debug.Log($"Saved data: {saveString}");
    }

    public void HidePanel()
    {
        popupPanel.SetActive(false);
    }

    public bool IsPanelActive()
    {
        return popupPanel.activeSelf;
    }

    public void UpdateSelectedModelIndex(int newIndex)
    {
        selectedModelIndex = newIndex;
        // Debug.Log("Selected Model Index Updated in SaveManager: " + selectedModelIndex);
    }

    public void Toast(string message)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("Toast: " + message);
            return;
        }

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, 0);
            toastObject.Call("show");
        }));
    }

    private void CloseButtonPressed()
    {
        PlayerPrefs.SetInt("OpenCloseButtonPressed", 1);
        PlayerPrefs.Save();
    }

    private void OpenButtonPressed()
    {
        PlayerPrefs.SetInt("OpenCloseButtonPressed", 0);
        PlayerPrefs.Save();
    }

    // ----------- EXPORT ALL ------------

    public void ExportAllSavesToFile()
    {
#if UNITY_ANDROID || UNITY_IOS
        var allModelsSaveData = new List<SaveDataWrapper>();

        // Assuming model indices 0 to 3 are valid
        for (int modelIndex = 0; modelIndex <= 3; modelIndex++)
        {
            var saveNames = GetAllSaveNames(modelIndex).ToList();
            var entries = new List<SaveEntry>();

            foreach (var name in saveNames)
            {
                string savedString = PlayerPrefs.GetString(GetSaveKey(modelIndex, name), null);
                if (string.IsNullOrEmpty(savedString)) continue;

                try
                {
                    int colonIndex = savedString.IndexOf(':');
                    int semicolonIndex = savedString.IndexOf(';');

                    if (colonIndex < 0 || semicolonIndex < 0) continue;

                    string valuesStr = savedString.Substring(colonIndex + 1, semicolonIndex - colonIndex - 1);
                    string dateStr = savedString.Substring(semicolonIndex + 1);

                    List<int> values = valuesStr.Split(',').Select(int.Parse).ToList();

                    entries.Add(new SaveEntry(name, values, dateStr));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed parsing save string for '{name}': {e.Message}");
                }
            }

            allModelsSaveData.Add(new SaveDataWrapper(modelIndex, entries));
        }

        if (allModelsSaveData.All(wrapper => wrapper.entries.Count == 0))
        {
            Toast("No saves to export.");
            return;
        }

        string json = JsonUtility.ToJson(new AllModelsSaveData(allModelsSaveData), true);
        string path = Path.Combine(Application.persistentDataPath, $"AllModelsSaves_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

        File.WriteAllText(path, json);
        Debug.Log($"Export saved to: {path}");

        NativeFilePicker.ExportFile(path, success =>
        {
            Toast(success ? "File exported!" : "File export failed.");
        });
#else
        Toast("Export only works on mobile.");
#endif
    }

    // ----------- IMPORT ALL ------------

    public void ImportAllSavesFromFile()
    {
#if UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.PickFile(path =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Toast("No file selected.");
                return;
            }
            HandleImportedAllModelsFile(path);
        }, new[] { "application/json" });
#else
        Toast("Import only works on mobile.");
#endif
    }

    private void HandleImportedAllModelsFile(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            AllModelsSaveData allModelsWrapper = JsonUtility.FromJson<AllModelsSaveData>(json);

            if (allModelsWrapper?.allModelsSaves == null || allModelsWrapper.allModelsSaves.Count == 0)
            {
                Toast("Imported file has no valid save data.");
                return;
            }

            // Optional: Clear all existing saves for models 0-3 before import
            ClearAllModelsSaves();

            foreach (var modelSaveWrapper in allModelsWrapper.allModelsSaves)
            {
                int modelIndex = modelSaveWrapper.modelIndex;
                if (modelSaveWrapper.entries == null) continue;

                var importedNames = new HashSet<string>();

                foreach (var entry in modelSaveWrapper.entries)
                {
                    if (entry.saveName == null || entry.values == null) continue;

                    string valuesStr = string.Join(",", entry.values);
                    string saveString = $"{entry.saveName}:{valuesStr};{entry.dateString}";

                    PlayerPrefs.SetString(GetSaveKey(modelIndex, entry.saveName), saveString);
                    importedNames.Add(entry.saveName);
                }

                // Update saved names list for this model index
                PlayerPrefs.SetString(GetSaveListKey(modelIndex), string.Join(",", importedNames));
            }

            PlayerPrefs.Save();

            Toast($"Imported saves for {allModelsWrapper.allModelsSaves.Count} models.");
            Debug.Log($"Imported all saves from file: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to import saves: {e.Message}");
            Toast("Failed to import saves.");
        }
    }

    private void ClearAllModelsSaves()
    {
        for (int modelIndex = 0; modelIndex <= 3; modelIndex++)
        {
            var names = GetAllSaveNames(modelIndex).ToList();
            foreach (var name in names)
            {
                PlayerPrefs.DeleteKey(GetSaveKey(modelIndex, name));
            }
            PlayerPrefs.SetString(GetSaveListKey(modelIndex), "");
        }
        PlayerPrefs.Save();
    }
}
