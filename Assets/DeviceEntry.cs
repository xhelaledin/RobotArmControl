// DeviceEntry.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DeviceEntry : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI macLabel;
    public TextMeshProUGUI selectedLabel; // This shows "Connected"
    public Image backgroundImage;
    public Button button;

    public void Setup(string deviceName, string macAddress, Sprite bgSprite, UnityAction onClick, bool isSelected)
    {
        nameLabel.text = deviceName;
        macLabel.text = "MAC: " + macAddress;

        if (backgroundImage != null && bgSprite != null)
            backgroundImage.sprite = bgSprite;

        // Show or hide the "Connected" label
        if (selectedLabel != null)
            selectedLabel.gameObject.SetActive(isSelected);

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        var colors = button.colors;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;
    }
}
