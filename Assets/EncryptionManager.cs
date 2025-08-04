using UnityEngine;
using TMPro;

public class EncryptionManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown encryptionDropdown;
    public TMP_InputField keyInputField;
    public TMP_Text keyStatusText;
    public GameObject keyPanel;

    [Header("Encryption Backends")]
    public AESEncryptionManager aesManager;
    public DESEncryptionManager desManager;

    private int encryptionTypeIndex = 0;

    private void Start()
    {
        encryptionTypeIndex = PlayerPrefs.GetInt("EncryptionTypeIndex", 0);
        encryptionDropdown.value = encryptionTypeIndex;
        encryptionDropdown.onValueChanged.AddListener(OnEncryptionChanged);

        aesManager.SetKey(PlayerPrefs.GetString("AESKey", ""));
        desManager.SetKey(PlayerPrefs.GetString("DESKey", ""));

        keyInputField.onValueChanged.AddListener(OnKeyInputChanged);

        UpdateKeyStatus();
    }

    public void OnEncryptionChanged(int index)
    {
        encryptionTypeIndex = index;
        PlayerPrefs.SetInt("EncryptionTypeIndex", index);
        PlayerPrefs.Save();

        keyInputField.characterLimit = GetRequiredKeyLength();

        if (encryptionTypeIndex == 0)
        {
            keyPanel.SetActive(false);
            keyStatusText.text = "Encryption: None (No Key Required)";
            keyStatusText.color = Color.gray;
        }
        else
        {
            ShowKeyPanel(); // Refresh with correct key
            UpdateKeyStatus();
        }
    }

    public void ShowKeyPanel()
    {
        keyPanel.SetActive(true);

        string currentKey = encryptionTypeIndex switch
        {
            1 => PlayerPrefs.GetString("AESKey", ""),
            2 => PlayerPrefs.GetString("DESKey", ""),
            _ => ""
        };

        keyInputField.characterLimit = GetRequiredKeyLength();
        keyInputField.text = currentKey;

        if (currentKey.Length == GetRequiredKeyLength())
        {
            keyStatusText.text = "Valid Key Loaded";
            keyStatusText.color = Color.green;
        }
        else
        {
            keyStatusText.text = $"Invalid Key (Need {GetRequiredKeyLength()} chars)";
            keyStatusText.color = Color.red;
        }
    }

    public void OnCancelClicked()
    {
        keyPanel.SetActive(false);
    }

    public void OnSetKeyClicked()
    {
        string inputKey = keyInputField.text;
        int requiredLength = GetRequiredKeyLength();

        if (inputKey.Length != requiredLength)
        {
            keyStatusText.text = $"Key must be {requiredLength} characters";
            keyStatusText.color = Color.red;
            return;
        }

        switch (encryptionTypeIndex)
        {
            case 1:
                aesManager.SetKey(inputKey);
                PlayerPrefs.SetString("AESKey", inputKey);
                break;
            case 2:
                desManager.SetKey(inputKey);
                PlayerPrefs.SetString("DESKey", inputKey);
                break;
        }

        PlayerPrefs.Save();
        keyPanel.SetActive(false);
        UpdateKeyStatus();
    }

    private void OnKeyInputChanged(string input)
    {
        int requiredLength = GetRequiredKeyLength();
        if (input.Length == requiredLength)
        {
            keyStatusText.text = "Valid Key";
            keyStatusText.color = Color.green;
        }
        else
        {
            keyStatusText.text = $"Invalid Key (Need {requiredLength} chars)";
            keyStatusText.color = Color.red;
        }
    }

    private int GetRequiredKeyLength() => encryptionTypeIndex switch
    {
        1 => aesManager.RequiredKeyLength,
        2 => desManager.RequiredKeyLength,
        _ => 0
    };

    private void UpdateKeyStatus()
    {
        bool hasKey = HasKey();
        keyInputField.interactable = encryptionTypeIndex != 0;

        if (encryptionTypeIndex == 0)
        {
            keyStatusText.text = "Encryption: None (No Key Required)";
            keyStatusText.color = Color.gray;
        }
        else if (hasKey)
        {
            string key = encryptionTypeIndex switch
            {
                1 => PlayerPrefs.GetString("AESKey", "[Hidden]"),
                2 => PlayerPrefs.GetString("DESKey", "[Hidden]"),
                _ => "[Unknown]"
            };

            keyStatusText.text = $"Key Set: {key}";
            keyStatusText.color = Color.green;
        }
        else
        {
            keyStatusText.text = "Key: Not Set";
            keyStatusText.color = Color.red;
        }
    }

    public bool HasKey() => encryptionTypeIndex switch
    {
        0 => true,
        1 => aesManager.HasKey(),
        2 => desManager.HasKey(),
        _ => false
    };

    public string EncryptString(string plain) => encryptionTypeIndex switch
    {
        0 => plain,
        1 => aesManager.EncryptString(plain),
        2 => desManager.EncryptString(plain),
        _ => plain
    };

    public string DecryptString(string encrypted) => encryptionTypeIndex switch
    {
        0 => encrypted,
        1 => aesManager.DecryptString(encrypted),
        2 => desManager.DecryptString(encrypted),
        _ => encrypted
    };
}
