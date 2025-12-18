using System.Collections.Generic;
using UnityEngine;

// Enums (PrefType, PrefCategory) are NOT defined here anymore 
// because they exist in your DataWrapper.cs file.

public static class PlayerPrefsKeyRegistry
{
    // Structure: Key -> (Type, Category, DefaultValue)
    public static readonly Dictionary<string, (PrefType type, PrefCategory category, object defaultValue)> KeyTypes =
        new Dictionary<string, (PrefType, PrefCategory, object)>
    {
        // --- Bluetooth Command Construct ---
        { "LastConnectedMAC", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "") },
        { "Slider1_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "S1") },
        { "Slider2_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "S2") },
        { "Slider3_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "S3") },
        { "Slider4_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "S4") },
        { "Slider5_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "S5") },
        { "Open_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "OPEN") },
        { "Close_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "CLOSE") },
        { "Save_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct, "SAVE") },
        { "Command_Delimiter", (PrefType.String, PrefCategory.BluetoothCommandConstruct, ":") },
        { "List_Delimiter", (PrefType.String, PrefCategory.BluetoothCommandConstruct, ",") },
        { "SingleModeToggle", (PrefType.Int, PrefCategory.BluetoothCommandConstruct, 0) },

        // --- Encryption ---
        { "KeyPref", (PrefType.String, PrefCategory.Encryption, "") },
        { "AESKey", (PrefType.String, PrefCategory.Encryption, "") },
        { "AES_Encryption_Key", (PrefType.String, PrefCategory.Encryption, "") },
        { "DESKey", (PrefType.String, PrefCategory.Encryption, "") },
        { "DES_Encryption_Key", (PrefType.String, PrefCategory.Encryption, "") },
        { "EncryptionTypeIndex", (PrefType.Int, PrefCategory.Encryption, 0) },

        // --- Slider Config (Defaults: Min=0, Max=180, Start=90, Flip=0) ---
        { "Slider1_Min", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "Slider1_Max", (PrefType.Int, PrefCategory.SliderConfig, 180) },
        { "Slider1_Start", (PrefType.Int, PrefCategory.SliderConfig, 90) },
        { "Slider1_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig, 0) },

        { "Slider2_Min", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "Slider2_Max", (PrefType.Int, PrefCategory.SliderConfig, 180) },
        { "Slider2_Start", (PrefType.Int, PrefCategory.SliderConfig, 90) },
        { "Slider2_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig, 0) },

        { "Slider3_Min", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "Slider3_Max", (PrefType.Int, PrefCategory.SliderConfig, 180) },
        { "Slider3_Start", (PrefType.Int, PrefCategory.SliderConfig, 90) },
        { "Slider3_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig, 0) },

        { "Slider4_Min", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "Slider4_Max", (PrefType.Int, PrefCategory.SliderConfig, 180) },
        { "Slider4_Start", (PrefType.Int, PrefCategory.SliderConfig, 90) },
        { "Slider4_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig, 0) },

        { "Slider5_Min", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "Slider5_Max", (PrefType.Int, PrefCategory.SliderConfig, 180) },
        { "Slider5_Start", (PrefType.Int, PrefCategory.SliderConfig, 90) },
        { "Slider5_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig, 0) },

        { "OpenButtonValue", (PrefType.Int, PrefCategory.SliderConfig, 105) },
        { "CloseButtonValue", (PrefType.Int, PrefCategory.SliderConfig, 177) },
        { "SendContinuously", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "SendIntervalStep", (PrefType.Int, PrefCategory.SliderConfig, 1) },
        
        { "OpenButtonPressed", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "CloseButtonPressed", (PrefType.Int, PrefCategory.SliderConfig, 0) },
        { "SelectedModelIndex", (PrefType.Int, PrefCategory.SliderConfig, 0) },

        // --- Model 4 Visuals (3 Parts) ---
        { "model4startRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model4startRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model4startRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model4directionpart1", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model4directionpart2", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model4directionpart3", (PrefType.Int, PrefCategory.Model3DVisual, 0) },

        // --- Model 5 Visuals (4 Parts) ---
        { "model5startRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5startRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5startRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5startRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5directionpart1", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model5directionpart2", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model5directionpart3", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model5directionpart4", (PrefType.Int, PrefCategory.Model3DVisual, 0) },

        // --- Model 5B Visuals (4 Parts) ---
        { "model5BstartRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5BstartRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5BstartRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5BstartRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model5Bdirectionpart1", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model5Bdirectionpart2", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model5Bdirectionpart3", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model5Bdirectionpart4", (PrefType.Int, PrefCategory.Model3DVisual, 0) },

        // --- Model 6 Visuals (5 Parts) ---
        { "model6startRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model6startRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model6startRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model6startRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model6startRotationpart5", (PrefType.Float, PrefCategory.Model3DVisual, 0f) },
        { "model6directionpart1", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model6directionpart2", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model6directionpart3", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model6directionpart4", (PrefType.Int, PrefCategory.Model3DVisual, 0) },
        { "model6directionpart5", (PrefType.Int, PrefCategory.Model3DVisual, 0) },

        // --- Bluetooth Logs ---
        { "TerminalLog", (PrefType.String, PrefCategory.BluetoothLogs, "") },
        { "ExportedLogFilePath", (PrefType.String, PrefCategory.BluetoothLogs, "") },
        { "ImportedLogFilePath", (PrefType.String, PrefCategory.BluetoothLogs, "") }
    };

    public static IEnumerable<string> Keys => KeyTypes.Keys;

    // --- SMART GETTERS ---
    public static int GetInt(string key)
    {
        if (KeyTypes.TryGetValue(key, out var data))
            return PlayerPrefs.GetInt(key, (int)data.defaultValue);
        return PlayerPrefs.GetInt(key); 
    }

    public static float GetFloat(string key)
    {
        if (KeyTypes.TryGetValue(key, out var data))
            return PlayerPrefs.GetFloat(key, (float)data.defaultValue);
        return PlayerPrefs.GetFloat(key);
    }

    public static string GetString(string key)
    {
        if (KeyTypes.TryGetValue(key, out var data))
            return PlayerPrefs.GetString(key, (string)data.defaultValue);
        return PlayerPrefs.GetString(key);
    }
}