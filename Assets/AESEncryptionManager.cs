using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System;

public class AESEncryptionManager : MonoBehaviour
{
    private byte[] key;
    private const string KeyPref = "AES_Encryption_Key";

    public int RequiredKeyLength => 16;

    private void Awake()
    {
        string storedKey = PlayerPrefs.GetString(KeyPref, "");
        if (!string.IsNullOrEmpty(storedKey) && storedKey.Length == RequiredKeyLength)
        {
            SetKey(storedKey);
            Debug.Log("AES Key loaded from PlayerPrefs.");
        }
        else
        {
            Debug.LogWarning("No valid AES key found in PlayerPrefs.");
        }
    }

    public void SetKey(string inputKey)
    {
        if (inputKey.Length != RequiredKeyLength)
        {
            Debug.LogError($"AES key must be exactly {RequiredKeyLength} characters.");
            key = null;
            return;
        }

        key = Encoding.UTF8.GetBytes(inputKey);
        PlayerPrefs.SetString(KeyPref, inputKey);
        PlayerPrefs.Save();
        Debug.Log("AES key saved.");
    }

    public bool HasKey() => key != null && key.Length == RequiredKeyLength;

    public string EncryptString(string plain)
    {
        if (!HasKey())
        {
            Debug.LogError("AES encryption failed: No valid key set.");
            return null;
        }

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            StringBuilder hex = new StringBuilder(encryptedBytes.Length * 2);
            foreach (byte b in encryptedBytes)
                hex.AppendFormat("{0:X2}", b);

            return hex.ToString();
        }
    }

    public string DecryptString(string encrypted)
    {
        if (!HasKey())
        {
            Debug.LogError("AES decryption failed: No valid key set.");
            return null;
        }

        byte[] encryptedBytes = new byte[encrypted.Length / 2];
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            encryptedBytes[i] = byte.Parse(encrypted.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
