using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_InputField inputField;
    public Button popupSaveButton, popupCancelButton;
    public Slider[] sliders;

    public Button closeButton;    // ✅ Button for "Close"
    public Button openButton;  // ✅ Button for "Open"

    private string defaultNamePrefix = "Position";

    private int selectedModelIndex;  // Store selected model index

    void Start()
    {
        popupPanel.SetActive(false);


        // ✅ Hook button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseButtonPressed);

        if (openButton != null)
            openButton.onClick.AddListener(OpenButtonPressed);

        // Get the selected model index from PlayerPrefs
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
        Debug.Log("Selected Model Index: " + selectedModelIndex);
    }

    private string GetSaveKey(string saveName)
    {
        // Use selectedModelIndex in the key to separate saves for each model
        return $"SavedArray_{selectedModelIndex}_{saveName}";
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        inputField.text = "";
    }

    public void StartSavingProcess()
    {
        string baseName = inputField.text.Trim();
        string initialName = string.IsNullOrWhiteSpace(baseName) ? GenerateDefaultName() : baseName;

        HashSet<string> existingNames = GetAllSaveNames();
        string saveName = GenerateUniqueName(initialName, existingNames);

        SaveArray(saveName);

        Toast("Saved as: " + saveName);
        ClosePopup();
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
        HashSet<string> names = GetAllSaveNames();
        string name;

        do
        {
            name = $"{defaultNamePrefix} {count}";
            count++;
        } while (names.Contains(name));

        return name;
    }

    private HashSet<string> GetAllSaveNames()
    {
        string raw = PlayerPrefs.GetString($"SaveList_{selectedModelIndex}", "");
        return new HashSet<string>(raw.Split(',').Where(n => !string.IsNullOrWhiteSpace(n)));
    }

    private void SaveArray(string saveName)
    {
        var names = GetAllSaveNames().ToList();

        if (!names.Contains(saveName))
        {
            names.Add(saveName);
            PlayerPrefs.SetString($"SaveList_{selectedModelIndex}", string.Join(",", names));
        }

        int[] saveValues = new int[5];
        for (int i = 0; i < 5; i++)
        {
            saveValues[i] = Mathf.RoundToInt(sliders[i].value);
        }

        bool setPressed = PlayerPrefs.GetInt("CloseButtonPressed", 0) == 1;
        saveValues[4] = setPressed ? 1 : 0;

        ArmSaveData data = new ArmSaveData { values = saveValues };
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(GetSaveKey(saveName), json);
        PlayerPrefs.Save();

        Debug.Log($"Saved array for \"{saveName}\": {string.Join(", ", saveValues)}");
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }


    public void UpdateSelectedModelIndex(int newIndex)
    {
        selectedModelIndex = newIndex;
        Debug.Log("Selected Model Index Updated in SaveManager: " + selectedModelIndex);
    }


    public void Toast(string message)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("Toast: " + message);
            return;
        }

        // Run on UI thread.
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
}
