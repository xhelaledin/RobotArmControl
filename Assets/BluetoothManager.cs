using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class BluetoothManager : MonoBehaviour
{
    [Header("UI Elements")]

    public TextMeshProUGUI connectionStatus;

    public GameObject scanningAnimation;
    public GameObject bluetoothMainPanel;

    [Header("Unified List UI")]
    public Transform pairedContentContainer;
    public Transform scannedContentContainer;
    public GameObject deviceEntryPrefab;
    public Sprite singleBackground;
    public Sprite firstBackground;
    public Sprite middleBackground;
    public Sprite lastBackground;
    public GameObject noDeviceEntryObject;

    [Header("Scan UI Controls")]
    public Button scanToggleButton;
    public TextMeshProUGUI scanToggleButtonText;

    private bool isScanning = false;
    private bool isConnected = false;
    private bool anyDeviceFound = false;

    // Store scanned devices in list of entries "Name\nMAC"
    private List<string> scannedDeviceList = new List<string>();

    // Android Bluetooth objects
    private AndroidJavaObject bluetoothAdapter;
    private AndroidJavaObject bluetoothSocket;

    private static AndroidJavaClass unity3dbluetoothplugin;
    private static AndroidJavaObject BluetoothConnector;

    private readonly string SPP_UUID = "00001101-0000-1000-8000-00805f9b34fb";

    [Header("External References")]
    public EncryptionManager encryptionManager;
    public Terminal terminal; // replaces old logger

    private string lastConnectedMAC = "";
    private string lastConnectedName = "";

    private string lastDisconnectedMAC = "";


    private List<string> lastPairedList = new List<string>();
    private List<string> lastScannedList = new List<string>();

    private void Awake()
    {
        lastConnectedMAC = "";
        lastConnectedName = "";
        PlayerPrefs.DeleteKey("LastConnectedMAC");
        PlayerPrefs.DeleteKey("LastConnectedName");
        PlayerPrefs.Save();
    }

    void Start()
    {
        InitBluetooth();
        InitPairedSection();
    }

    public void InitBluetooth()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        // Request all necessary permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation) ||
            !Permission.HasUserAuthorizedPermission(Permission.FineLocation) ||
            !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH") ||
            !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN") ||
            !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN") ||
            !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {
            Permission.RequestUserPermissions(new string[]
            {
                Permission.CoarseLocation,
                Permission.FineLocation,
                "android.permission.BLUETOOTH",
                "android.permission.BLUETOOTH_ADMIN",
                "android.permission.BLUETOOTH_SCAN",
                "android.permission.BLUETOOTH_CONNECT"
            });
        }

        var adapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter");
        bluetoothAdapter = adapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");

        unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
        BluetoothConnector = unity3dbluetoothplugin.CallStatic<AndroidJavaObject>("getInstance");

        if (bluetoothAdapter == null)
        {
            Toast("Bluetooth not supported on this device");
            return;
        }

        UpdateConnectionStatus("Status: Disconnected");
    }

    public void ShowBluetoothPanel()
    {
        bluetoothMainPanel.SetActive(true);
        StartScanUI();

        lastConnectedMAC = PlayerPrefs.GetString("LastConnectedMAC", "");
        lastConnectedName = PlayerPrefs.GetString("LastConnectedName", "");
    }

    public void HideBluetoothPanel()
    {
        bluetoothMainPanel.SetActive(false);
        StopScanUI();
    }

    public void OnScanToggleButtonPressed()
    {
        if (isScanning) StopScanUI();
        else StartScanUI();

        if (noDeviceEntryObject != null)
            noDeviceEntryObject.SetActive(false);

    }

    private void StartScanUI()
    {
        if (isConnected == true) return;

        // Reset scan state
        anyDeviceFound = false;

        if (noDeviceEntryObject != null)
            noDeviceEntryObject.SetActive(false); // hide the message at start

        scannedDeviceList.Clear();
        PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);

        StartScan();
        isScanning = true;
        scanToggleButtonText.text = "Stop";
        if (scanningAnimation != null)
            scanningAnimation.SetActive(true);
    }


    private void StopScanUI()
    {
        StopScanDevices();
        isScanning = false;
        scanToggleButtonText.text = "Scan";
        if (scanningAnimation != null)
            scanningAnimation.SetActive(false);
    }

    public void InitPairedSection()
    {
        GetPairedDevices();
    }

    public void GetPairedDevices()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        AndroidJavaObject pairedDevices = bluetoothAdapter.Call<AndroidJavaObject>("getBondedDevices");
        var list = new List<string>();

        if (pairedDevices != null)
        {
            int deviceCount = pairedDevices.Call<int>("size");
            if (deviceCount > 0)
            {
                var iterator = pairedDevices.Call<AndroidJavaObject>("iterator");
                while (iterator.Call<bool>("hasNext"))
                {
                    var device = iterator.Call<AndroidJavaObject>("next");
                    string name = device.Call<string>("getName");
                    string address = device.Call<string>("getAddress");
                    list.Add($"{name}\n{address}");
                }
            }
        }

        PopulateList(list, pairedContentContainer, OnDeviceSelected);
    }

    public void StartScan()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        if (isConnected == true)
        {
            return;
        }
        else
        {
            scannedDeviceList.Clear();
            PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);
            BluetoothConnector.CallStatic("StartScanDevices");
        }
    }

    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        BluetoothConnector.CallStatic("StopScanDevices");
    }

    // Called by Java when a new device is found during scan
    public void NewDeviceFound(string data)
    {
        Debug.Log("NewDeviceFound: " + data);
        if (string.IsNullOrEmpty(data)) return;

        var parts = data.Split('+');
        if (parts.Length < 2) return;

        string address = parts[0];
        string name = parts[1];
        string entry = $"{name}\n{address}";

        if (scannedDeviceList.Contains(entry)) return;

        scannedDeviceList.Add(entry);
        anyDeviceFound = true;

        PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);
    }


    public void ScanStatus(string status)
    {
        Debug.Log("ScanStatus: " + status);
        //Toast("Scan Status: " + status);

        if (status == "stopped" || status == "completed")
        {
            isScanning = false;
            scanToggleButtonText.text = "Scan";
            if (scanningAnimation != null)
                scanningAnimation.SetActive(false);

            PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);

            if (!anyDeviceFound && noDeviceEntryObject != null)
            {
                noDeviceEntryObject.SetActive(true);
            }
        }
    }


    // public void OnDeviceSelected(string address)
    // {
    //     if (isConnected && address == lastConnectedMAC)
    //     {
    //         StopConnection();
    //     }
    //     else
    //     {
    //         StartConnection(address);
    //     }
    // }

    public void OnDeviceSelected(string address)
    {
        var deviceEntry = FindDeviceEntryByMAC(address, pairedContentContainer);
        if (deviceEntry != null)
            deviceEntry.SetConnectionStatus("Connecting...", Color.yellow);

        if (isConnected && address == lastConnectedMAC)
        {
            StopConnection();
        }
        else
        {
            StartConnection(address);
        }
    }

    public void OnScannedDeviceSelected(string address)
    {
        var deviceEntry = FindDeviceEntryByMAC(address, scannedContentContainer);
        if (deviceEntry != null)
            deviceEntry.SetConnectionStatus("Connecting...", Color.yellow);

        if (isConnected && address == lastConnectedMAC)
        {
            StopConnection();
        }
        else
        {
            StartConnection(address);
        }
    }


    // public void OnScannedDeviceSelected(string address)
    // {
    //     if (isConnected && address == lastConnectedMAC)
    //     {
    //         StopConnection();
    //     }
    //     else
    //     {
    //         StartConnection(address);
    //     }
    // }


    public void StartConnection(string deviceAddress)
    {
        lastDisconnectedMAC = "";
        if (Application.platform != RuntimePlatform.Android)
        {
            Toast("Platform not Android.");
            return;
        }

        if (string.IsNullOrEmpty(deviceAddress))
        {
            Toast("No device address provided.");
            return;
        }

        try
        {
            // Get the remote device
            AndroidJavaObject device = bluetoothAdapter.Call<AndroidJavaObject>("getRemoteDevice", deviceAddress);
            if (device == null)
            {
                Toast("Failed to get device with address: " + deviceAddress);
                return;
            }

            // Get the SPP UUID
            AndroidJavaClass uuidClass = new AndroidJavaClass("java.util.UUID");
            AndroidJavaObject sppUUID = uuidClass.CallStatic<AndroidJavaObject>("fromString", SPP_UUID);

            // Create RFCOMM socket and connect
            bluetoothSocket = device.Call<AndroidJavaObject>("createRfcommSocketToServiceRecord", sppUUID);
            bluetoothSocket.Call("connect");

            isConnected = true;
            StopScanUI();
            lastConnectedMAC = deviceAddress;
            lastConnectedName = FindDeviceNameByMAC(deviceAddress);

            PlayerPrefs.SetString("LastConnectedMAC", lastConnectedMAC);
            PlayerPrefs.SetString("LastConnectedName", lastConnectedName);
            PlayerPrefs.Save();

            // Toast("Connected to " + lastConnectedName);
            Toast("Connected to " + FindDeviceNameByMAC(lastConnectedMAC));


            UpdateConnectionStatus("Status: Connected to " + lastConnectedName);

            // Update UI for paired devices list
            InitPairedSection();
        }
        catch (Exception ex)
        {
            isConnected = false;
            Toast("Connection failed: " + ex.Message);
            UpdateConnectionStatus("Status: Connection failed");
            Debug.LogError("Connection failed: " + ex);
        }
    }

    // Called by Java when connection status changes (or you can trigger it yourself)
    public void ConnectionStatus(string status)
    {
        Debug.Log("ConnectionStatus: " + status);
        UpdateConnectionStatus("Status: " + status);

        if (status == "connected")
        {
            isConnected = true;
            //HideBluetoothPanel();

            // Toast($"Connected to: {lastConnectedName}");
            Toast("Connected to " + FindDeviceNameByMAC(lastConnectedMAC));

            PlayerPrefs.SetString("LastConnectedMAC", lastConnectedMAC);
            PlayerPrefs.SetString("LastConnectedName", lastConnectedName);
            PlayerPrefs.Save();

            InitPairedSection(); // update UI
        }
        else if (status == "disconnected" || status == "unable to connect")
        {
            isConnected = false;
        }
    }


    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        string disconnectedDeviceName = lastConnectedName;
        lastDisconnectedMAC = lastConnectedMAC;  // Remember disconnected MAC for UI
        lastConnectedMAC = "";
        lastConnectedName = "";
        isConnected = false;

        UpdateConnectionStatus("Status: Disconnected");
        // Toast($"Disconnected from {disconnectedDeviceName}");
        Toast("Disconnected from " + FindDeviceNameByMAC(disconnectedDeviceName));

        try
        {
            if (bluetoothSocket != null)
            {
                bluetoothSocket.Call("close");
                bluetoothSocket = null;
            }
        }
        catch (Exception ex)
        {
            Toast("Disconnect failed: " + ex.Message);
            Debug.LogError("Disconnect failed: " + ex);
        }

        try
        {
            BluetoothConnector.CallStatic("StopConnection");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Java StopConnection call failed (ignored): " + ex.Message);
        }

        // Refresh UI lists to update connection labels
        PopulateList(lastPairedList, pairedContentContainer, OnDeviceSelected);
        PopulateList(lastScannedList, scannedContentContainer, OnDeviceSelected);
    }

    private void ClearConnectedLabel(Transform container, string macAddress)
    {
        foreach (Transform child in container)
        {
            var comp = child.GetComponent<DeviceEntry>();
            if (comp != null && comp.MACAddress == macAddress)
            {
                comp.SetConnectionStatus("");
            }
        }
    }


    // Called by Java when data is received
    public void ReadData(string data)
    {
        Debug.Log("BT Stream: " + data);
        if (terminal != null)
            terminal.LogReceived(data);
        else
            Debug.LogWarning("Terminal reference missing, cannot log received data.");
    }

    /// <summary>
    /// Writes encrypted data over the socket's output stream.
    /// Uses old connection logic from BluetoothManagerOld.
    /// </summary>
    public void WriteData(string plainText)
    {
        if (Application.platform != RuntimePlatform.Android || !isConnected)
            return;

        if (string.IsNullOrWhiteSpace(plainText))
        {
            Toast("No data to send");
            return;
        }

        try
        {
            if (bluetoothSocket == null)
            {
                Debug.LogError("Bluetooth socket is null.");
                Toast("Bluetooth socket is not connected.");
                return;
            }

            if (encryptionManager == null)
            {
                Debug.LogError("Encryption manager is null.");
                Toast("Encryption system not ready.");
                return;
            }

            string encryptedHex = encryptionManager.EncryptString(plainText);
            byte[] dataBytes = Encoding.UTF8.GetBytes(encryptedHex);

            // Append newline
            byte[] finalBytes = new byte[dataBytes.Length + 1];
            Buffer.BlockCopy(dataBytes, 0, finalBytes, 0, dataBytes.Length);
            finalBytes[finalBytes.Length - 1] = (byte)'\n';

            AndroidJavaObject outputStream = bluetoothSocket.Call<AndroidJavaObject>("getOutputStream");
            outputStream.Call("write", finalBytes);
            outputStream.Call("flush");

            Toast($"Encrypted + Sent: {plainText} as {encryptedHex}");

            if (terminal != null)
                terminal.LogSent(plainText);
            else
                Debug.LogWarning("Terminal reference missing, cannot log sent data.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Bluetooth Write Failed: " + ex);
            Toast("Write failed: " + ex.Message);
            StopConnection();
        }
    }

    // public void ResetPosition()
    // {
    //     WriteData("SERVOS:0,90,90,105");
    // }

    public void SaveDeviceMAC(string mac)
    {
        PlayerPrefs.SetString("LastConnectedMAC", mac);
        PlayerPrefs.Save();
    }

    public void Toast(string message)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("Toast: " + message);
            return;
        }

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, 0);
            toastObject.Call("show");
        }));
    }

    public void UpdateConnectionStatus(string status)
    {
        if (connectionStatus != null)
            connectionStatus.text = status;
    }

    private string FindDeviceNameByMAC(string mac)
    {
        foreach (var entry in scannedDeviceList)
        {
            if (entry.Contains(mac))
                return entry.Split('\n')[0];
        }
        return mac; // fallback
    }

    private void PopulateList(List<string> devices, Transform container, Action<string> onDeviceSelected)
    {
        if (container == pairedContentContainer)
            lastPairedList = new List<string>(devices);
        else if (container == scannedContentContainer)
            lastScannedList = new List<string>(devices);

        foreach (Transform child in container)
            Destroy(child.gameObject);

        int count = devices.Count;

        for (int i = 0; i < count; i++)
        {
            var parts = devices[i].Split('\n');
            string deviceName = parts[0];
            string macAddress = parts.Length > 1 ? parts[1] : parts[0];

            var go = Instantiate(deviceEntryPrefab, container);
            var comp = go.GetComponent<DeviceEntry>();
            if (comp == null) continue;

            Sprite bg;
            if (count == 1) bg = singleBackground;
            else if (i == 0) bg = firstBackground;
            else if (i == count - 1) bg = lastBackground;
            else bg = middleBackground;

            bool isSelected = macAddress == lastConnectedMAC;
            comp.Setup(deviceName, macAddress, bg, () => onDeviceSelected(macAddress), isSelected);

            if (isSelected)
            {
                comp.SetConnectionStatus("Connected", Color.green);
            }
            else if (macAddress == lastDisconnectedMAC)
            {
                comp.SetConnectionStatus("Disconnected", Color.red);
                // Clear lastDisconnectedMAC after showing once
                lastDisconnectedMAC = "";
            }
            else
            {
                comp.SetConnectionStatus("");
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container);
        StartCoroutine(ResizeParentHeight(container));
    }


    // Optional: auto-resize wrapper height (if using a max height cap)
    private IEnumerator ResizeParentHeight(Transform container)
    {
        yield return null;

        RectTransform contentRT = (RectTransform)container;
        RectTransform parentRT = (RectTransform)container.parent;

        float preferredHeight = LayoutUtility.GetPreferredHeight(contentRT);
        parentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
    }
    
    private DeviceEntry FindDeviceEntryByMAC(string mac, Transform container)
    {
        foreach (Transform child in container)
        {
            var entry = child.GetComponent<DeviceEntry>();
            if (entry != null && entry.MACAddress == mac)
                return entry;
        }
        return null;
    }

}
