using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AesEncryptor : MonoBehaviour
{
    private byte[] key;
    private const string KeyPref = "AES_Encryption_Key";

    private void Awake()
    {
        // Load the key from PlayerPrefs if it exists
        string storedKey = PlayerPrefs.GetString(KeyPref, "");
        if (!string.IsNullOrEmpty(storedKey) && storedKey.Length == 16)
        {
            SetKey(storedKey);
            Debug.Log("Key loaded from PlayerPrefs.");
        }
        else
        {
            Debug.LogWarning("No valid key found in PlayerPrefs.");
        }
    }

    public void SetKey(string inputKey)
    {
        if (inputKey.Length != 16)
        {
            Debug.LogError("AES key must be exactly 16 characters for AES-128.");
            return;
        }
        
        key = Encoding.UTF8.GetBytes(inputKey);
        PlayerPrefs.SetString(KeyPref, inputKey);
        PlayerPrefs.Save();
        Debug.Log("Key saved to PlayerPrefs.");
    }

    public bool HasKey()
    {
        return key != null && key.Length == 16;
    }

    public string GetKeyPreview()
    {
        return key != null ? Encoding.UTF8.GetString(key) : "Key not set";
    }

    public string EncryptString(string plain)
    {
        if (!HasKey())
        {
            Debug.LogError("Encryption attempted without a valid key.");
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
            Debug.LogError("Decryption attempted without a valid key.");
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
