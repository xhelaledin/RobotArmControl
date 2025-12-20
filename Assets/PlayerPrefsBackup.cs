using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using NativeFilePickerNamespace;

// NOTE: 'Entry' and 'DataWrapper' classes are defined in your DataWrapper.cs file.
// This script simply uses them.

public class PlayerPrefsBackup : MonoBehaviour
{
    [Header("Panels")]
    public GameObject backupRestorePanel;   // Root panel holding backup & restore
    public GameObject backupPanel;          // Backup subpanel
    public GameObject restorePanel;         // Restore subpanel

    [Header("Category Manager")]
    public CategorySelectionManager categorySelectionManager;

    private DataWrapper lastImportedData;

    // === BACKUP ===
    public void SavePrefsWithSelectedCategories(List<PrefCategory> selectedCategories)
    {
        var entries = new List<Entry>();
        int found = 0;

        // Loop through the REGISTRY to ensure defaults are included
        foreach (var kvp in PlayerPrefsKeyRegistry.KeyTypes)
        {
            string key = kvp.Key;
            var (type, category, defVal) = kvp.Value;

            if (!selectedCategories.Contains(category))
                continue;

            // Get value using Registry Smart Getters (returns Default if Pref doesn't exist)
            string valueStr = null;
            switch (type)
            {
                case PrefType.Int:
                    valueStr = PlayerPrefsKeyRegistry.GetInt(key).ToString();
                    break;
                case PrefType.Float:
                    valueStr = PlayerPrefsKeyRegistry.GetFloat(key).ToString("F6");
                    break;
                case PrefType.String:
                    valueStr = PlayerPrefsKeyRegistry.GetString(key);
                    break;
            }

            if (valueStr != null)
            {
                // Using your Entry constructor: Entry(key, value, type, category)
                entries.Add(new Entry(key, valueStr, type.ToString(), category.ToString()));
                found++;
            }
        }

        if (found == 0)
        {
            Debug.LogWarning("[PlayerPrefsBackup] No keys found in registry for selected categories.");
            return;
        }

        // Using your DataWrapper constructor: DataWrapper(List<Entry>)
        string json = JsonUtility.ToJson(new DataWrapper(entries), true);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.persistentDataPath, $"RobotArmControl_backup_{timestamp}.json");

        File.WriteAllText(path, json);
        Debug.Log($"[PlayerPrefsBackup] Backup saved: {path} (Entries: {found})");

#if UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.ExportFile(path, success =>
        {
            Debug.Log(success ? "[PlayerPrefsBackup] File exported!" : "[PlayerPrefsBackup] File export failed.");
        });
#endif
    }

    // === BACKUP PANEL ===
    public void ShowBackupPanel()
    {
        if (backupRestorePanel == null || backupPanel == null || restorePanel == null || categorySelectionManager == null)
        {
            Debug.LogError("[PlayerPrefsBackup] UI references missing!");
            return;
        }

        backupRestorePanel.SetActive(true);
        backupPanel.SetActive(true);

        var allCategories = PlayerPrefsKeyRegistry.KeyTypes.Values
            .Select(v => v.category)
            .Distinct()
            .ToList();

        var itemDataList = allCategories.Select(cat => new CategorySelectionManager.ItemData
        {
            itemName = cat.ToString(),
            prefCategory = cat,
            category = (int)cat
        }).ToList();

        categorySelectionManager.RefreshToggleList(itemDataList);
        categorySelectionManager.ShowBackupPanel();

        PanelManager.Instance.PushPanel(
            key: backupPanel,
            hide: () => HideBackupPanel(),
            isActive: () => backupPanel != null && backupPanel.activeSelf
        );
    }

    public void HideBackupPanel() => backupPanel?.SetActive(false);

    // === BACKUP-RESTORE ROOT PANEL ===
    public void ShowBackupRestorePanel()
    {
        if (backupRestorePanel == null)
        {
            Debug.LogError("[PlayerPrefsBackup] UI references missing!");
            return;
        }

        backupRestorePanel.SetActive(true);

        PanelManager.Instance.PushPanel(
            key: backupRestorePanel,
            hide: () => HideBackupRestorePanel(),
            isActive: () => backupRestorePanel != null && backupRestorePanel.activeSelf
        );
    }

    public void HideBackupRestorePanel() => backupRestorePanel?.SetActive(false);

    // === RESTORE PANEL ===
    public void ShowRestorePanel()
    {
        if (restorePanel == null)
        {
            Debug.LogError("[PlayerPrefsBackup] Restore panel reference missing!");
            return;
        }

        backupRestorePanel?.SetActive(true);
        restorePanel.SetActive(true);

        PanelManager.Instance.PushPanel(
            key: restorePanel,
            hide: () => HideRestorePanel(),
            isActive: () => restorePanel != null && restorePanel.activeSelf
        );
    }

    public void HideRestorePanel() => restorePanel?.SetActive(false);

    // === RESTORE LOGIC ===
    public void RestorePrefsFromFilePicker()
    {
#if UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.PickFile(path =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[PlayerPrefsBackup] No file selected for restore.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<DataWrapper>(json);

                if (wrapper?.entries == null || wrapper.entries.Count == 0)
                {
                    Debug.LogWarning("[PlayerPrefsBackup] Backup file has no data.");
                    return;
                }

                lastImportedData = wrapper;

                var availableCategories = GetCategoriesInData(wrapper);

                backupRestorePanel.SetActive(true);
                backupPanel.SetActive(false);
                restorePanel.SetActive(true);

                var itemDataList = availableCategories.Select(cat => new CategorySelectionManager.ItemData
                {
                    itemName = cat.ToString(),
                    prefCategory = cat,
                    category = (int)cat
                }).ToList();

                categorySelectionManager.RefreshToggleList(itemDataList);
                categorySelectionManager.ShowRestorePanel(path, availableCategories);

                ShowRestorePanel();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsBackup] Restore failed: {ex.Message}");
            }
        }, new[] { "application/json", "text/plain" });
#else
        Debug.LogWarning("[PlayerFsBackup] Restore via file picker only works on mobile builds.");
#endif
    }

    public void RestorePrefsFromFileWithSelectedCategories(string path, List<PrefCategory> selectedCategories)
    {
        if (lastImportedData == null)
        {
            Debug.LogWarning("[PlayerPrefsBackup] No imported data found.");
            return;
        }

        int restored = 0;

        foreach (var entry in lastImportedData.entries)
        {
            if (!Enum.TryParse(entry.category, out PrefCategory category)) continue;
            if (!selectedCategories.Contains(category)) continue;

            switch (entry.type)
            {
                case "Int":
                    if (int.TryParse(entry.value, out int i))
                        PlayerPrefs.SetInt(entry.key, i);
                    break;

                case "Float":
                    if (float.TryParse(entry.value, out float f))
                        PlayerPrefs.SetFloat(entry.key, f);
                    break;

                case "String":
                    PlayerPrefs.SetString(entry.key, entry.value ?? "");
                    break;
            }

            restored++;
        }

        PlayerPrefs.Save();
        Debug.Log($"[PlayerPrefsBackup] PlayerPrefs restored. Total restored: {restored}");

        HideBackupRestorePanel();
        HideRestorePanel();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private List<PrefCategory> GetCategoriesInData(DataWrapper data)
    {
        var categories = new HashSet<PrefCategory>();
        foreach (var entry in data.entries)
        {
            if (Enum.TryParse(entry.category, out PrefCategory cat))
                categories.Add(cat);
        }
        return new List<PrefCategory>(categories);
    }
}