using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BluetoothLoggerUI : MonoBehaviour
{
    public GameObject logPrefab;  // Assign the LogEntry prefab in the Inspector (must have a TextMeshProUGUI component)
    public Transform logContainer;  // Assign the Content GameObject in the Inspector (inside the ScrollRect)
    private List<GameObject> logEntries = new List<GameObject>();

    private void Start()
    {
        // Check if everything is assigned
        if (logPrefab == null)
            Debug.LogError("[BluetoothLoggerUI] logPrefab is not assigned in the Inspector.");
        else
            Debug.Log("[BluetoothLoggerUI] logPrefab assigned correctly.");

        if (logContainer == null)
            Debug.LogError("[BluetoothLoggerUI] logContainer is not assigned in the Inspector.");
        else
            Debug.Log("[BluetoothLoggerUI] logContainer assigned correctly.");
    }

    public void LogMessage(string message)
    {
        Debug.Log($"[BluetoothLoggerUI] Attempting to log message: '{message}'");

        if (logPrefab == null || logContainer == null)
        {
            Debug.LogError("[BluetoothLoggerUI] logPrefab or logContainer is missing. Cannot log message.");
            return;
        }

        // Instantiate a new log entry
        GameObject newLogEntry = Instantiate(logPrefab, logContainer);
        Debug.Log("[BluetoothLoggerUI] Instantiated new log entry.");

        var textComponent = newLogEntry.GetComponent<TextMeshProUGUI>();

        if (textComponent == null)
        {
            Debug.LogError("[BluetoothLoggerUI] Log prefab does not contain a TextMeshProUGUI component. Destroying the log entry.");
            Destroy(newLogEntry);
            return;
        }

        textComponent.text = message;
        Debug.Log($"[BluetoothLoggerUI] Set message text: '{message}'");

        // Add it to the list
        logEntries.Add(newLogEntry);
        Debug.Log($"[BluetoothLoggerUI] Added log entry. Total entries: {logEntries.Count}");

        // Scroll to the bottom
        //ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        var scrollRect = logContainer?.GetComponentInParent<ScrollRect>();

        if (scrollRect == null)
        {
            Debug.LogWarning("[BluetoothLoggerUI] ScrollRect not found. Make sure the log container is properly assigned.");
            return;
        }

        scrollRect.verticalNormalizedPosition = 0f;
        Debug.Log("[BluetoothLoggerUI] Scrolled to bottom.");
    }

    public void ClearLog()
    {
        Debug.Log("[BluetoothLoggerUI] Clearing log entries...");

        foreach (GameObject entry in logEntries)
        {
            Destroy(entry);
        }

        logEntries.Clear();
        Debug.Log("[BluetoothLoggerUI] Log entries cleared.");
    }
}
