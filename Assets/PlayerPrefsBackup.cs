using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using NativeFilePickerNamespace;

public class PlayerPrefsBackup : MonoBehaviour
{
    public GameObject backupRestorePanel;
    public GameObject backupPanel;
    public GameObject restorePanel;
    public CategorySelectionManager categorySelectionManager;

    private DataWrapper lastImportedData;

    // === Backup ===

    public void SavePrefsWithSelectedCategories(List<PrefCategory> selectedCategories)
    {
        var entries = new List<Entry>();
        int found = 0, skipped = 0;

        foreach (var kvp in PlayerPrefsKeyRegistry.KeyTypes)
        {
            string key = kvp.Key;
            var (type, category) = kvp.Value;

            if (!selectedCategories.Contains(category)) continue;

            if (!PlayerPrefs.HasKey(key))
            {
                Debug.Log($"❌ Key not present in PlayerPrefs: {key}");
                continue;
            }

            string value = null;
            switch (type)
            {
                case PrefType.Int:
                    value = PlayerPrefs.GetInt(key).ToString();
                    break;
                case PrefType.Float:
                    value = PlayerPrefs.GetFloat(key).ToString("F6");
                    break;
                case PrefType.String:
                    value = PlayerPrefs.GetString(key);
                    break;
            }

            if (!string.IsNullOrEmpty(value))
            {
                entries.Add(new Entry(key, value, type.ToString(), category.ToString()));
                Debug.Log($"📝 Saved: {key} = {value} ({type})");
                found++;
            }
            else
            {
                Debug.Log($"⚠️ Skipped key (empty value): {key}");
                skipped++;
            }
        }

        if (found == 0)
        {
            Debug.LogWarning("⚠️ No valid PlayerPrefs values found to back up.");
            return;
        }

        string json = JsonUtility.ToJson(new DataWrapper(entries), true);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.persistentDataPath, $"RobotArmControl_backup_{timestamp}.json");

        File.WriteAllText(path, json);
        Debug.Log($"✅ Backup saved: {path} (saved: {found}, skipped: {skipped})");

#if UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.ExportFile(path, success =>
        {
            Debug.Log(success ? "📤 File exported!" : "❌ File export failed.");
        });
#endif
    }

    public void ShowBackupPanel()
    {
        backupPanel?.SetActive(true);
        categorySelectionManager.ShowBackupPanel(); // triggers toggle UI
    }

    public void HideBackupPanel() => backupPanel?.SetActive(false);

    // === Restore ===

    public void RestorePrefsFromFilePicker()
    {
#if UNITY_ANDROID || UNITY_IOS
        NativeFilePicker.PickFile(path =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("❌ No file selected for restore.");
                return;
            }

            Debug.Log($"📂 Restore file: {path}");
            try
            {
                string json = File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<DataWrapper>(json);

                if (wrapper?.entries == null || wrapper.entries.Count == 0)
                {
                    Debug.LogWarning("⚠️ Backup file has no data.");
                    return;
                }

                lastImportedData = wrapper;

                // Ask user which categories to restore
                var availableCategories = GetCategoriesInData(wrapper);
                categorySelectionManager.ShowRestorePanel(path, availableCategories);
                restorePanel?.SetActive(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Restore failed: {ex.Message}");
            }
        }, new[] { "application/json", "text/plain" });
#else
        Debug.LogWarning("⚠️ Restore via file picker only works on mobile builds.");
#endif
    }

    public void RestorePrefsFromFileWithSelectedCategories(string path, List<PrefCategory> selectedCategories)
    {
        if (lastImportedData == null)
        {
            Debug.LogWarning("⚠️ No imported data found.");
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
                    else
                        Debug.LogWarning($"⚠️ Failed to parse int for key: {entry.key}");
                    break;

                case "Float":
                    if (float.TryParse(entry.value, out float f))
                        PlayerPrefs.SetFloat(entry.key, f);
                    else
                        Debug.LogWarning($"⚠️ Failed to parse float for key: {entry.key}");
                    break;

                case "String":
                    PlayerPrefs.SetString(entry.key, entry.value ?? "");
                    break;

                default:
                    Debug.LogWarning($"⚠️ Unknown type: {entry.type} for key: {entry.key}");
                    break;
            }

            Debug.Log($"🔄 Restored: {entry.key} = {entry.value} ({entry.type})");
            restored++;
        }

        PlayerPrefs.Save();
        Debug.Log($"✅ PlayerPrefs restored. Total restored: {restored}");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowRestorePanel() => restorePanel?.SetActive(true);
    public void HideRestorePanel() => restorePanel?.SetActive(false);

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

    public void ShowBackupRestorePanel() => backupRestorePanel?.SetActive(true);
    public void HideBackupRestorePanel() => backupRestorePanel?.SetActive(false);
}
