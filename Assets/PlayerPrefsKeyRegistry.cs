using System.Collections.Generic;

public static class PlayerPrefsKeyRegistry
{
    public static readonly Dictionary<string, (PrefType type, PrefCategory category)> KeyTypes =
        new Dictionary<string, (PrefType, PrefCategory)>
    {
        // Bluetooth Command Construct
        { "LastConnectedMAC", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Slider1_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Slider2_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Slider3_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Slider4_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Slider5_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Open_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Close_Command", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "Command_Delimiter", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "List_Delimiter", (PrefType.String, PrefCategory.BluetoothCommandConstruct) },
        { "SingleModeToggle", (PrefType.Int, PrefCategory.BluetoothCommandConstruct) },

        // Encryption
        { "KeyPref", (PrefType.String, PrefCategory.Encryption) },
        { "AESKey", (PrefType.String, PrefCategory.Encryption) },
        { "AES_Encryption_Key", (PrefType.String, PrefCategory.Encryption) },
        { "DESKey", (PrefType.String, PrefCategory.Encryption) },
        { "DES_Encryption_Key", (PrefType.String, PrefCategory.Encryption) },
        { "EncryptionTypeIndex", (PrefType.Int, PrefCategory.Encryption) },

        // Slider Config
        { "Slider1_Min", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider1_Max", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider1_Start", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider1_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider2_Min", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider2_Max", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider2_Start", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider2_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider3_Min", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider3_Max", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider3_Start", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider3_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider4_Min", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider4_Max", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider4_Start", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider4_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider5_Min", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider5_Max", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider5_Start", (PrefType.Int, PrefCategory.SliderConfig) },
        { "Slider5_FlipDirection", (PrefType.Int, PrefCategory.SliderConfig) },
        { "OpenButtonValue", (PrefType.Int, PrefCategory.SliderConfig) },
        { "CloseButtonValue", (PrefType.Int, PrefCategory.SliderConfig) },
        { "SendContinuously", (PrefType.Int, PrefCategory.SliderConfig) },
        { "SendIntervalStep", (PrefType.Int, PrefCategory.SliderConfig) },

        // General Saved with slider config
        { "OpenButtonPressed", (PrefType.Int, PrefCategory.SliderConfig) },
        { "CloseButtonPressed", (PrefType.Int, PrefCategory.SliderConfig) },
        { "SelectedModelIndex", (PrefType.Int, PrefCategory.SliderConfig) },

        // 3D Model Visual Start Rotations and Directions
        { "model4startRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model4startRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model4startRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model4startRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model4startRotationpart5", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model4directionpart1", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model4directionpart2", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model4directionpart3", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model4directionpart4", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model4directionpart5", (PrefType.Int, PrefCategory.Model3DVisual) },

        { "model5startRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5startRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5startRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5startRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5startRotationpart5", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5directionpart1", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5directionpart2", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5directionpart3", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5directionpart4", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5directionpart5", (PrefType.Int, PrefCategory.Model3DVisual) },

        { "model5BstartRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5BstartRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5BstartRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5BstartRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5BstartRotationpart5", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model5Bdirectionpart1", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5Bdirectionpart2", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5Bdirectionpart3", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5Bdirectionpart4", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model5Bdirectionpart5", (PrefType.Int, PrefCategory.Model3DVisual) },

        { "model6startRotationpart1", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model6startRotationpart2", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model6startRotationpart3", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model6startRotationpart4", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model6startRotationpart5", (PrefType.Float, PrefCategory.Model3DVisual) },
        { "model6directionpart1", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model6directionpart2", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model6directionpart3", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model6directionpart4", (PrefType.Int, PrefCategory.Model3DVisual) },
        { "model6directionpart5", (PrefType.Int, PrefCategory.Model3DVisual) },

        // Bluetooth Logs
        { "TerminalLog", (PrefType.String, PrefCategory.BluetoothLogs) },
        { "ExportedLogFilePath", (PrefType.String, PrefCategory.BluetoothLogs) },
        { "ImportedLogFilePath", (PrefType.String, PrefCategory.BluetoothLogs) }
    };

    public static IEnumerable<string> Keys => KeyTypes.Keys;

    public static PrefType GetType(string key) => KeyTypes[key].type;
    public static PrefCategory GetCategory(string key) => KeyTypes[key].category;
}
