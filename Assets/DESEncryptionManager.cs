using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System;

public class DESEncryptionManager : MonoBehaviour
{
    private byte[] key;
    private const string KeyPref = "DES_Encryption_Key";

    public int RequiredKeyLength => 8;

    private void Awake()
    {
        string storedKey = PlayerPrefs.GetString(KeyPref, "");
        if (!string.IsNullOrEmpty(storedKey) && storedKey.Length == RequiredKeyLength)
        {
            SetKey(storedKey);
            Debug.Log("DES Key loaded from PlayerPrefs.");
        }
        else
        {
            Debug.LogWarning("No valid DES key found in PlayerPrefs.");
        }
    }

    public void SetKey(string inputKey)
    {
        if (inputKey.Length != RequiredKeyLength)
        {
            Debug.LogError($"DES key must be exactly {RequiredKeyLength} characters.");
            key = null;
            return;
        }

        key = Encoding.UTF8.GetBytes(inputKey);
        PlayerPrefs.SetString(KeyPref, inputKey);
        PlayerPrefs.Save();
        Debug.Log("DES key saved.");
    }

    public bool HasKey() => key != null && key.Length == RequiredKeyLength;

    public string EncryptString(string plain)
    {
        if (!HasKey())
        {
            Debug.LogError("DES encryption failed: No valid key set.");
            return null;
        }

        using (DES des = DES.Create())
        {
            des.Key = key;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = des.CreateEncryptor();
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
            Debug.LogError("DES decryption failed: No valid key set.");
            return null;
        }

        byte[] encryptedBytes = new byte[encrypted.Length / 2];
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            encryptedBytes[i] = byte.Parse(encrypted.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }

        using (DES des = DES.Create())
        {
            des.Key = key;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = des.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
