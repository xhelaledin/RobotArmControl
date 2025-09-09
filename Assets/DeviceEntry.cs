using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DeviceEntry : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI macLabel;
    public TextMeshProUGUI connectionStatusText; // Show "Connected" or "Disconnected"

    public Image backgroundImage;
    public Button button;

    public string MACAddress { get; private set; } // Store MAC for matching

    public void Setup(string deviceName, string macAddress, Sprite bgSprite, UnityAction onClick, bool isSelected)
    {
        nameLabel.text = deviceName;
        macLabel.text = "MAC: " + macAddress;
        MACAddress = macAddress; // store it

        if (backgroundImage != null && bgSprite != null)
            backgroundImage.sprite = bgSprite;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        var colors = button.colors;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        // Hide connectionStatusText initially
        if (connectionStatusText != null)
        {
            connectionStatusText.gameObject.SetActive(false);
            connectionStatusText.color = Color.black; // default color
        }

        // Show "Connected" if selected
        if (isSelected)
            SetConnectionStatus("Connected", Color.green);
    }

    /// <summary>
    /// Set the connection status text and optionally color it.
    /// Pass empty string to hide the status text.
    /// </summary>
    public void SetConnectionStatus(string status, Color? color = null)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = status;
            connectionStatusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
            connectionStatusText.color = color ?? Color.black; // default black if no color given
        }
    }
}
