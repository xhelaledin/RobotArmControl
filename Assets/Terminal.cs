using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Terminal : MonoBehaviour, IDeselectHandler
{
    [Header("UI References")]
    public GameObject terminalPanel;
    public TMP_InputField inputField;
    public Transform messageContainer;
    public GameObject sentMessagePrefab;
    public GameObject receivedMessagePrefab;
    public GameObject dateGroupPrefab;
    public ScrollRect scrollRect;


    [Header("World Space Keyboard Handling")]
    public Canvas worldCanvas;
    public RectTransform inputBarTransform;
    public RectTransform scrollViewContainer;

    private Vector3 inputBarOriginalPosition;
    private Vector3 scrollViewOriginalPosition;

    private const string logKey = "TerminalLog";
    private string lastMessageDate = "";

    private bool allowDefocus = false;

    [Header("Connection to Other Script")]
    public BluetoothManager bluetoothManager;

    private void Start()
    {
        if (inputBarTransform == null)
            inputBarTransform = inputField.transform.parent.GetComponent<RectTransform>();

        if (inputBarTransform != null)
            inputBarOriginalPosition = inputBarTransform.localPosition;

        if (scrollViewContainer == null && scrollRect != null)
            scrollViewContainer = scrollRect.GetComponent<RectTransform>();

        if (scrollViewContainer != null)
            scrollViewOriginalPosition = scrollViewContainer.localPosition;

        StartCoroutine(DelayedInputActivation());
        LoadLogFromPrefs();
    }

    private void Update()
    {
        AdjustInputBarForKeyboard();
    }

    private IEnumerator DelayedInputActivation()
    {
        yield return new WaitForSeconds(0.2f);
        if (inputField != null)
        {
            inputField.interactable = true;
            inputField.ActivateInputField();
        }
    }

    public void ShowTerminalPanel()
    {
        terminalPanel?.SetActive(true);
        inputField.interactable = true;
        inputField.ActivateInputField();
        StartCoroutine(ScrollToBottomNextFrame());

        PanelManager.Instance.PushPanel(
            key: terminalPanel,
            hide: HidePanel,      // Pass the existing HidePanel method
            isActive: IsPanelActive  // Pass the existing IsPanelActive method
        );
    }

    public void HidePanel()
    {
        terminalPanel?.SetActive(false);
        allowDefocus = true;
        if (inputField != null)
            inputField.DeactivateInputField();
    }

    public bool IsPanelActive()
    {
        if (terminalPanel == null) return false;
        return terminalPanel.activeSelf;
    }

    public void SendMessageFromInput()
    {
        string message = inputField.text;
        if (string.IsNullOrWhiteSpace(message)) return;

        if (bluetoothManager != null)
            bluetoothManager.WriteData(message);

        LogSent(message);
        inputField.text = "";
        inputField.ActivateInputField(); // Keep keyboard open and input focused after send
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!allowDefocus && inputField != null)
        {
            inputField.ActivateInputField();
        }
    }

    public void CloseKeyboard()
    {
        allowDefocus = true;
        if (inputField != null)
            inputField.DeactivateInputField();
    }

    public void LogSent(string message)
    {
        DateTime now = DateTime.Now;
        AddMessage(message, now, true);
        SaveMessageToPrefs("[SENT]", message, now);
    }

    public void LogReceived(string message)
    {
        DateTime now = DateTime.Now;
        AddMessage(message, now, false);
        SaveMessageToPrefs("[RECV]", message, now);
    }

    private void AddMessage(string message, DateTime timestamp, bool isSent)
    {
        bool isCurrentYear = timestamp.Year == DateTime.Now.Year;
        string dateFormat = isCurrentYear
            ? timestamp.ToString("dddd, MMMM dd")
            : timestamp.ToString("dddd, MMMM dd yyyy");

        if (lastMessageDate != dateFormat)
        {
            lastMessageDate = dateFormat;
            GameObject dateGroup = Instantiate(dateGroupPrefab, messageContainer);
            var dateTMP = dateGroup.GetComponentInChildren<TextMeshProUGUI>();
            if (dateTMP != null)
                dateTMP.text = dateFormat;
        }

        GameObject prefab = isSent ? sentMessagePrefab : receivedMessagePrefab;
        GameObject msgGO = Instantiate(prefab, messageContainer);
        RectTransform bubbleRT = msgGO.GetComponent<RectTransform>();
        RectTransform contentRT = msgGO.transform.GetChild(0).GetComponent<RectTransform>();

        var messageTMP = contentRT.GetChild(0).GetComponent<TextMeshProUGUI>();
        var timeTMP = contentRT.GetChild(1).GetComponent<TextMeshProUGUI>();

        messageTMP.text = message;
        timeTMP.text = timestamp.ToString("HH:mm");

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

        float msgHeight = messageTMP.GetPreferredValues(message, contentRT.rect.width, 0f).y;
        float timeHeight = timeTMP.GetPreferredValues(timeTMP.text, contentRT.rect.width, 0f).y;

        float topPadding = 10f;
        float betweenPadding = 5f;
        float bottomPadding = 30f;

        float totalHeight = topPadding + msgHeight + betweenPadding + timeHeight + bottomPadding;

        contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        bubbleRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        float halfH = totalHeight * 0.5f;

        messageTMP.rectTransform.anchoredPosition = new Vector2(
            messageTMP.rectTransform.anchoredPosition.x,
            halfH - topPadding - (msgHeight * 0.5f)
        );

        timeTMP.rectTransform.anchoredPosition = new Vector2(
            timeTMP.rectTransform.anchoredPosition.x,
            halfH - topPadding - msgHeight - betweenPadding - (timeHeight * 0.5f)
        );

        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)messageContainer);
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        ScrollToBottom();
    }

    private void SaveMessageToPrefs(string prefix, string message, DateTime timestamp)
    {
        var existing = LoadLogList();
        string toSave = $"{prefix}|{timestamp:yyyy-MM-dd HH:mm:ss}|{message.Replace("|", "/")}";
        existing.Add(toSave);
        PlayerPrefs.SetString(logKey, string.Join("||", existing));
        PlayerPrefs.Save();
    }

    private List<string> LoadLogList()
    {
        string raw = PlayerPrefs.GetString(logKey, "");
        if (string.IsNullOrEmpty(raw)) return new List<string>();
        return new List<string>(raw.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries));
    }

    private void LoadLogFromPrefs()
    {
        var log = LoadLogList();
        foreach (var entry in log)
        {
            var parts = entry.Split('|');
            if (parts.Length < 3) continue;
            if (!DateTime.TryParse(parts[1], out DateTime ts)) continue;
            bool sent = parts[0] == "[SENT]";
            AddMessage(parts[2], ts, sent);
        }
        ScrollToBottom();
    }

    public void ClearLog()
    {
        PlayerPrefs.DeleteKey(logKey);
        foreach (Transform c in messageContainer)
            Destroy(c.gameObject);
        lastMessageDate = "";
    }

    private void AdjustInputBarForKeyboard()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
            using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
            {
                AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
                decorView.Call("getWindowVisibleDisplayFrame", rect);

                int visibleHeight = rect.Call<int>("height");
                int totalHeight = Screen.height;
                int keyboardHeight = totalHeight - visibleHeight;

                if (keyboardHeight > totalHeight * 0.15f)
                {
                    float canvasHeight = worldCanvas.GetComponent<RectTransform>().rect.height;
                    float yRatio = keyboardHeight / (float)Screen.height;
                    float worldOffset = canvasHeight * yRatio;

                    inputBarTransform.localPosition = inputBarOriginalPosition + new Vector3(0, worldOffset, 0);
                    scrollViewContainer.localPosition = scrollViewOriginalPosition + new Vector3(0, worldOffset, 0);
                }
                else
                {
                    inputBarTransform.localPosition = inputBarOriginalPosition;
                    scrollViewContainer.localPosition = scrollViewOriginalPosition;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Keyboard detection failed: " + e.Message);
        }
#endif
    }
}