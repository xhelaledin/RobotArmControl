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
    public TMP_Text toastText;
    public float toastDuration = 2f;

    public Button setButton;    // ✅ Button for "Close"
    public Button unsetButton;  // ✅ Button for "Open"

    private string defaultNamePrefix = "Position";
    private Coroutine toastCoroutine;

    void Start()
    {
        popupPanel.SetActive(false);

        if (toastText != null)
        {
            toastText.gameObject.SetActive(false);
            var cg = toastText.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
        }

        // ✅ Hook button listeners
        if (setButton != null)
            setButton.onClick.AddListener(SetButtonPressed);

        if (unsetButton != null)
            unsetButton.onClick.AddListener(UnsetButtonPressed);

        Debug.Log("Start(): SetButtonPressed = " + PlayerPrefs.GetInt("SetButtonPressed", 0));
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        inputField.text = "";
    }

    public void StartSavingProcess()
    {
        Debug.Log("StartSavingProcess clicked! ✅");
        string baseName = inputField.text.Trim();
        string initialName = string.IsNullOrWhiteSpace(baseName) ? GenerateDefaultName() : baseName;

        HashSet<string> existingNames = GetAllSaveNames();
        string saveName = GenerateUniqueName(initialName, existingNames);

        SaveArray(saveName);

        //ShowToast($"Saved as \"{saveName}\"");
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
        string raw = PlayerPrefs.GetString("SaveList", "");
        return new HashSet<string>(raw.Split(',').Where(n => !string.IsNullOrWhiteSpace(n)));
    }

    private void SaveArray(string saveName)
    {
        var names = GetAllSaveNames().ToList();

        if (!names.Contains(saveName))
        {
            names.Add(saveName);
            PlayerPrefs.SetString("SaveList", string.Join(",", names));
        }

        int[] saveValues = new int[4];
        for (int i = 0; i < 3; i++)
        {
            saveValues[i] = Mathf.RoundToInt(sliders[i].value);
        }

        // ✅ Use saved button state
        bool setPressed = PlayerPrefs.GetInt("SetButtonPressed", 0) == 1;
        saveValues[3] = setPressed ? 1 : 0;

        Debug.Log($"💾 Reading SetButtonPressed: {PlayerPrefs.GetInt("SetButtonPressed", 0)} → Saving as {saveValues[3]}");

        ArmSaveData data = new ArmSaveData { values = saveValues };
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString($"SavedArray_{saveName}", json);
        PlayerPrefs.Save();

        Debug.Log($"✅ Saved array for \"{saveName}\": {string.Join(", ", saveValues)}");
    }

    public void ClosePopup()
    {
        Debug.Log("ClosePopup clicked! ✅");
        popupPanel.SetActive(false);
    }

    public void ShowToast(string message)
    {
        if (toastCoroutine != null) StopCoroutine(toastCoroutine);
        toastCoroutine = StartCoroutine(ShowToastCoroutine(message));
    }

    /// <summary>
    /// Displays a Toast using Android's Toast API.
    /// </summary>
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

    private IEnumerator ShowToastCoroutine(string message)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);

        CanvasGroup canvasGroup = toastText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = toastText.gameObject.AddComponent<CanvasGroup>();
        }

        float fadeTime = 0.3f;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeTime;
            yield return null;
        }

        canvasGroup.alpha = 1;
        yield return new WaitForSeconds(toastDuration);

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1 - (t / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0;
        toastText.gameObject.SetActive(false);
    }

    // ✅ Called automatically by buttons (wired in Start)

    private void SetButtonPressed()
    {
        PlayerPrefs.SetInt("SetButtonPressed", 1);
        PlayerPrefs.Save();
        Debug.Log("🔴 SetButtonPressed() called — SetButtonPressed = 1 (Closed)");
    }

    private void UnsetButtonPressed()
    {
        PlayerPrefs.SetInt("SetButtonPressed", 0);
        PlayerPrefs.Save();
        Debug.Log("🟢 UnsetButtonPressed() called — SetButtonPressed = 0 (Open)");
    }
}

