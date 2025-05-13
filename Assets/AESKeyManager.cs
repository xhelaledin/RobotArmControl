using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class AESKeyManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject keyPanel;
    public TMP_InputField keyInputField;
    public TMP_Text keyStatusText;

    [Header("Encryption Settings")]
    [SerializeField] private AesEncryptor aesEncryptor;  // Now visible in the inspector

    private void Start()
    {
        // Fallback in case you forget to assign it in the inspector
        if (aesEncryptor == null)
        {
            aesEncryptor = GetComponent<AesEncryptor>();
            if (aesEncryptor == null)
            {
                Debug.LogError("AesEncryptor component is missing. Please attach it to the same GameObject or assign it in the inspector.");
                return;
            }
        }

        keyPanel.SetActive(false);
        UpdateKeyStatus();
    }

    public void OnSetKeyClicked()
    {
        string keyText = keyInputField.text;

        if (string.IsNullOrEmpty(keyText) || keyText.Length != 16)
        {
            Debug.LogError("Key must be exactly 16 characters for AES-128");
            keyStatusText.text = "Invalid Key (Need 16 chars)";
            keyStatusText.color = Color.red;
            return;
        }

        aesEncryptor.SetKey(keyText);
        UpdateKeyStatus();
        keyPanel.SetActive(false);
    }

    public void OnCancelClicked()
    {
        keyPanel.SetActive(false);
    }

    public void ShowKeyPanel()
    {
        keyPanel.SetActive(true);
        keyInputField.text = "";
        keyInputField.characterLimit = 16;  // 16 characters for AES-128
    }

    private void UpdateKeyStatus()
    {
        if (aesEncryptor.HasKey())
        {
            keyStatusText.text = aesEncryptor.GetKeyPreview();
            keyStatusText.color = Color.green;
        }
        else
        {
            keyStatusText.text = "Key: Not Set";
            keyStatusText.color = Color.red;
        }
    }
}
