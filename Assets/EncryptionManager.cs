using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EncryptionManager : MonoBehaviour
{
    [Header("UI Components")]
    public AdvancedDropdown encryptionDropdown; 
    public TMP_InputField keyInputField;
    public TMP_Text keyStatusText;
    public GameObject keyPanel;

    [Header("Panel Buttons")]
    public GameObject button0; // Shown when index 0 selected
    public GameObject button1; // Shown when index 1 selected
    public GameObject button2; // Shown when index 2 selected

    [Header("Encryption Backends")]
    public AESEncryptionManager aesManager;
    public DESEncryptionManager desManager;

    private int encryptionTypeIndex = 0;

    private void Start()
    {
        // USING REGISTRY FOR DEFAULTS
        encryptionTypeIndex = PlayerPrefsKeyRegistry.GetInt("EncryptionTypeIndex");

        if (encryptionDropdown != null)
        {
            encryptionDropdown.SelectOption(encryptionTypeIndex);
            encryptionDropdown.onChangedValue += OnEncryptionChanged;
        }

        // USING REGISTRY FOR DEFAULTS
        if (aesManager != null)
            aesManager.SetKey(PlayerPrefsKeyRegistry.GetString("AESKey"));
        if (desManager != null)
            desManager.SetKey(PlayerPrefsKeyRegistry.GetString("DESKey"));

        if (keyInputField != null)
        {
            keyInputField.onValueChanged.AddListener(OnKeyInputChanged);
            keyInputField.onEndEdit.AddListener(_ => OnSetKeyClicked());
        }

        UpdateKeyStatus();
        UpdateButtonVisibility();
    }

    private void OnDestroy()
    {
        if (encryptionDropdown != null)
            encryptionDropdown.onChangedValue -= OnEncryptionChanged;
    }

    public void OnEncryptionChanged(int index)
    {
        encryptionTypeIndex = index;
        PlayerPrefs.SetInt("EncryptionTypeIndex", index);
        PlayerPrefs.Save();

        keyInputField.characterLimit = GetRequiredKeyLength();
        UpdateButtonVisibility();

        if (encryptionTypeIndex == 0)
        {
            // Hide input field but keep the panel open
            keyInputField.gameObject.SetActive(false);
            keyStatusText.text = "Encryption: None (No Key Required)";
            keyStatusText.color = Color.gray;
        }
        else
        {
            keyInputField.gameObject.SetActive(true);
            ShowKeyPanel();
            UpdateKeyStatus();
        }
    }

    private void UpdateButtonVisibility()
    {
        button0?.SetActive(encryptionTypeIndex == 0);
        button1?.SetActive(encryptionTypeIndex == 1);
        button2?.SetActive(encryptionTypeIndex == 2);
    }

    public void ShowKeyPanel()
    {
        keyPanel.SetActive(true);

        PanelManager.Instance.PushPanel(
            key: keyPanel,
            hide: HidePanel,      // Pass the existing HidePanel method
            isActive: IsPanelActive  // Pass the existing IsPanelActive method
        );
        

        // Handle "None" encryption type
        if (encryptionTypeIndex == 0)
        {
            keyInputField.gameObject.SetActive(false);
            keyStatusText.text = "Encryption: None (No Key Required)";
            keyStatusText.color = Color.gray;
            return;
        }

        // Otherwise, show input field and load saved key
        keyInputField.gameObject.SetActive(true);

        // USING REGISTRY FOR DEFAULTS
        string currentKey = encryptionTypeIndex switch
        {
            1 => PlayerPrefsKeyRegistry.GetString("AESKey"),
            2 => PlayerPrefsKeyRegistry.GetString("DESKey"),
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

    // This method is now called by PanelManager's 'hide' delegate
    public void HidePanel()
    {
        keyPanel.SetActive(false);
    }

    // This method is now called by PanelManager's 'isActive' delegate
    public bool IsPanelActive()
    {
        if (keyPanel == null) return false;
        return keyPanel.activeSelf;
    }

    public void OnSetKeyClicked()
    {
        if (encryptionTypeIndex == 0) return;

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
        UpdateKeyStatus();
    }

    private void OnKeyInputChanged(string input)
    {
        if (encryptionTypeIndex == 0)
        {
            // No validation for "None" encryption
            keyStatusText.text = "Encryption: None (No Key Required)";
            keyStatusText.color = Color.gray;
            return;
        }

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
            // USING REGISTRY FOR DEFAULTS
            string key = encryptionTypeIndex switch
            {
                1 => PlayerPrefsKeyRegistry.GetString("AESKey"),
                2 => PlayerPrefsKeyRegistry.GetString("DESKey"),
                _ => "[Unknown]"
            };

            // To avoid showing the key, let's just confirm it's set
            keyStatusText.text = $"Key Set"; // Simplified: "Key Set: {key}"
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