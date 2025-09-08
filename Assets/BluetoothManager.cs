using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;
using UnityEngine.EventSystems;

public class BluetoothManager : MonoBehaviour, IHideablePanel
{
    [Header("UI Elements")]
    public TextMeshProUGUI connectionStatus;
    public GameObject settingsPanel;
    public GameObject bluetoothMainPanel;
    public GameObject bluetoothHandlePanel;
    public GameObject bluetoothEnablePanel;

    [Header("Bluetooth Status Button")]
    public Button mainPageBluetoothButton;
    public Button listPanelBluetoothButton;
    public Sprite disconnectedSprite;
    public Sprite connectedSprite;

    [Header("Unified List UI")]
    public GameObject deviceEntryPrefab;
    public Sprite singleBackground;
    public Sprite firstBackground;
    public Sprite middleBackground;
    public Sprite lastBackground;
    public GameObject noDeviceEntryObject;
    public GameObject handleNoDeviceEntryObject;

    // Original panel containers
    public Transform pairedContentContainer;
    public Transform scannedContentContainer;

    // Optional new panel containers
    public Transform extraPairedContentContainer;
    public Transform extraScannedContentContainer;

    [Header("Scan UI Controls")]
    public Button scanToggleButton;
    public TextMeshProUGUI scanToggleButtonText;
    public GameObject scanningAnimation;

    public Button scanHandleToggleButton;
    public TextMeshProUGUI scanHandleToggleButtonText;
    public GameObject scanningHandleAnimation;
    

    private bool isScanning = false;
    private bool isConnected = false;
    private bool anyDeviceFound = false;

    private List<string> scannedDeviceList = new List<string>();
    private AndroidJavaObject bluetoothAdapter;
    private AndroidJavaObject bluetoothSocket;

    private static AndroidJavaClass unity3dbluetoothplugin;
    private static AndroidJavaObject BluetoothConnector;
    private readonly string SPP_UUID = "00001101-0000-1000-8000-00805f9b34fb";

    [Header("External References")]
    public EncryptionManager encryptionManager;
    public Terminal terminal;

    private string lastConnectedMAC = "";
    private string lastConnectedName = "";
    private string lastDisconnectedMAC = "";

    private List<string> lastPairedList = new List<string>();
    private List<string> lastScannedList = new List<string>();

    private bool mainPageButtonFlag;
    private bool bluetoothHandlePanelFlag;
    private bool bluetoothEnablePanelFlag;

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

        // Get BluetoothAdapter
        using (var adapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        {
            bluetoothAdapter = adapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");
        }

        if (bluetoothAdapter == null)
        {
            Toast("Bluetooth not supported on this device");
            return;
        }

        // Check if Bluetooth is enabled
        bool isEnabled = bluetoothAdapter.Call<bool>("isEnabled");
        if (!isEnabled)
        {
            // Show system dialog to enable Bluetooth
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intentClass = new AndroidJavaClass("android.content.Intent"))
            {
                string ACTION_REQUEST_ENABLE = "android.bluetooth.adapter.action.REQUEST_ENABLE";
                var intent = new AndroidJavaObject("android.content.Intent", ACTION_REQUEST_ENABLE);
                activity.Call("startActivity", intent);
            }

            UpdateConnectionStatus("Status: Bluetooth off");
            return;
        }

        // Bluetooth is enabled → continue with connector init
        unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
        BluetoothConnector = unity3dbluetoothplugin.CallStatic<AndroidJavaObject>("getInstance");

        UpdateConnectionStatus("Status: Disconnected");
    }


        // Enum for clarity
    private enum BluetoothPanel
    {
        DefaultPanel = 0,
        MainPagePanel = 1,
        HandlePanel = 2
    }

    // Entry functions
    public void ShowEnablePanel()
    {
        StartCoroutine(EnableBluetoothRoutine(BluetoothPanel.DefaultPanel));
    }

    public void ShowEnablePanelForMainPage()
    {
        StartCoroutine(EnableBluetoothRoutine(BluetoothPanel.MainPagePanel));
    }

    public void ShowEnablePanelHandle()
    {
        StartCoroutine(EnableBluetoothRoutine(BluetoothPanel.HandlePanel));
    }

    private IEnumerator EnableBluetoothRoutine(BluetoothPanel panel)
    {
        InitBluetooth(); // Show system enable dialog if Bluetooth is off

        // Wait for user to respond (with optional timeout)
        float timeout = 10f; // seconds
        float timer = 0f;

        while (!IsBluetoothEnabled() && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!IsBluetoothEnabled())
        {
            Debug.Log("User did not enable Bluetooth.");
            yield break; // Exit if Bluetooth still disabled
        }

        // Bluetooth enabled → continue
        InitPairedSection();

        // Show the correct panel
        switch (panel)
        {
            case BluetoothPanel.HandlePanel:
                ShowBluetoothHandlePanel();
                break;
            case BluetoothPanel.MainPagePanel:
                ShowBluetoothPanelFromMainPage();
                break;
            default:
                ShowBluetoothPanel();
                break;
        }
    }

    private bool IsBluetoothEnabled()
    {
        if (bluetoothAdapter == null) return false;
        return bluetoothAdapter.Call<bool>("isEnabled");
    }

    // Panel methods (InitBluetooth removed; handled in coroutine)
    public void ShowBluetoothPanel()
    {
        InitPairedSection();
        bluetoothMainPanel.SetActive(true);
        mainPageButtonFlag = false;
        noDeviceEntryObject.SetActive(false);
        StartScanUI(pairedContentContainer, scannedContentContainer);
        LoadLastConnection();
        PanelManager.Instance.RegisterPanel(this);
    }

    public void ShowBluetoothPanelFromMainPage()
    {
        InitPairedSection();
        settingsPanel.SetActive(true);
        bluetoothMainPanel.SetActive(true);
        mainPageButtonFlag = true;
        noDeviceEntryObject.SetActive(false);
        StopScanUI();
        StopScanDevices();
        LoadLastConnection();
        scannedDeviceList.Clear();
        PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);
        PanelManager.Instance.RegisterPanel(this);
    }

    public void ShowBluetoothHandlePanel()
    {
        InitPairedSection();
        bluetoothHandlePanel.SetActive(true);
        bluetoothHandlePanelFlag = true;
        noDeviceEntryObject.SetActive(false);
        scannedDeviceList.Clear();
        PopulateList(scannedDeviceList, extraScannedContentContainer, OnScannedDeviceSelected);
        LoadLastConnection();
        PanelManager.Instance.RegisterPanel(this);
    }

    private void LoadLastConnection()
    {
        lastConnectedMAC = PlayerPrefs.GetString("LastConnectedMAC", "");
        lastConnectedName = PlayerPrefs.GetString("LastConnectedName", "");
    }

    public void HidePanel()
    {
        if (bluetoothHandlePanelFlag)
        {
            bluetoothHandlePanel.SetActive(false);
            bluetoothHandlePanelFlag = false;
        }
        else if (bluetoothEnablePanelFlag)
        {
            bluetoothEnablePanel.SetActive(false);
            bluetoothEnablePanelFlag = false;
        }
        else if (!mainPageButtonFlag)
            bluetoothMainPanel.SetActive(false);
        else
        {
            settingsPanel.SetActive(false);
            bluetoothMainPanel.SetActive(false);
        }
        StopScanUI();
    }

    // public bool IsPanelActive() => bluetoothMainPanel.activeSelf;

    public bool IsPanelActive()
{
        return bluetoothMainPanel.activeSelf ||
               bluetoothHandlePanel.activeSelf;
}

    public void UpdateButtonSprite(bool state)
    {
        mainPageBluetoothButton.image.sprite = state ? connectedSprite : disconnectedSprite;
        listPanelBluetoothButton.image.sprite = state ? connectedSprite : disconnectedSprite;
    }

    public void OnScanToggleButtonPressed()
    {
        if (isScanning) StopScanUI();
        else StartScanUI(pairedContentContainer, scannedContentContainer);

        if (noDeviceEntryObject != null)
            noDeviceEntryObject.SetActive(false);

        if (handleNoDeviceEntryObject != null)
            handleNoDeviceEntryObject.SetActive(false);
    }

    private void StartScanUI(Transform pairedContainer, Transform scannedContainer)
    {
        if (isConnected) return;

        anyDeviceFound = false;
        scannedDeviceList.Clear();

        if (noDeviceEntryObject != null)
            noDeviceEntryObject.SetActive(false);

        if (handleNoDeviceEntryObject != null)
            handleNoDeviceEntryObject.SetActive(false);

        PopulateList(scannedDeviceList, scannedContainer, OnScannedDeviceSelected);

        StartScan();
        isScanning = true;
        scanToggleButtonText.text = "Stop";
        scanHandleToggleButtonText.text = "Stop Scanning";

        if (scanningAnimation != null)
            scanningAnimation.SetActive(true);

        if (scanningHandleAnimation != null)
            scanningHandleAnimation.SetActive(true);
    }

    private void StopScanUI()
    {
        StopScanDevices();
        isScanning = false;
        scanToggleButtonText.text = "Scan";
        scanHandleToggleButtonText.text = "Start Scanning";
        if (scanningAnimation != null)
            scanningAnimation.SetActive(false);

        if (scanningHandleAnimation != null)
            scanningHandleAnimation.SetActive(false);
    }

    public void InitPairedSection()
    {
        GetPairedDevices(pairedContentContainer);
        if(extraPairedContentContainer != null)
            GetPairedDevices(extraPairedContentContainer);
    }

    public void GetPairedDevices(Transform container)
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

        PopulateList(list, container, OnDeviceSelected);
    }

    public void StartScan()
    {
        if (Application.platform != RuntimePlatform.Android || isConnected) return;

        scannedDeviceList.Clear();
        PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);
        if(extraScannedContentContainer != null)
        PopulateList(scannedDeviceList, extraScannedContentContainer, OnScannedDeviceSelected);

        BluetoothConnector.CallStatic("StartScanDevices");
    }

    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android) return;
        BluetoothConnector.CallStatic("StopScanDevices");
    }

    public void NewDeviceFound(string data)
    {
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
        if(extraScannedContentContainer != null)
            PopulateList(scannedDeviceList, extraScannedContentContainer, OnScannedDeviceSelected);
    }

    public void ScanStatus(string status)
    {
        if (status == "stopped" || status == "completed")
        {
            isScanning = false;
            scanToggleButtonText.text = "Scan";
            scanHandleToggleButtonText.text = "Start Scanning";
            if (scanningAnimation != null)
                scanningAnimation.SetActive(false);

            if (scanningHandleAnimation != null)
                scanningHandleAnimation.SetActive(false);

            PopulateList(scannedDeviceList, scannedContentContainer, OnScannedDeviceSelected);

            PopulateList(scannedDeviceList, extraScannedContentContainer, OnScannedDeviceSelected);

            if (!anyDeviceFound && noDeviceEntryObject && handleNoDeviceEntryObject != null)
            {
                noDeviceEntryObject.SetActive(true);
                handleNoDeviceEntryObject.SetActive(true);
            }
        }
    }

        public void OnDeviceSelected(string address)
    {
        var entryMain = FindDeviceEntryByMAC(address, pairedContentContainer);
        var entryExtra = FindDeviceEntryByMAC(address, extraPairedContentContainer);

        if (entryMain != null)
            entryMain.SetConnectionStatus("Connecting...", Color.yellow);

        if (entryExtra != null)
            entryExtra.SetConnectionStatus("Connecting...", Color.yellow);

        if (isConnected && address == lastConnectedMAC) StopConnection();
        else StartConnection(address);
    }

    public void OnScannedDeviceSelected(string address)
    {
        var entryMain = FindDeviceEntryByMAC(address, scannedContentContainer);
        var entryExtra = FindDeviceEntryByMAC(address, extraScannedContentContainer);

        if (entryMain != null)
            entryMain.SetConnectionStatus("Connecting...", Color.yellow);

        if (entryExtra != null)
            entryExtra.SetConnectionStatus("Connecting...", Color.yellow);

        if (isConnected && address == lastConnectedMAC) StopConnection();
        else StartConnection(address);
    }


    // ------------------ Connection Methods ------------------ //

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
            AndroidJavaObject device = bluetoothAdapter.Call<AndroidJavaObject>("getRemoteDevice", deviceAddress);
            if (device == null)
            {
                Toast("Failed to get device with address: " + deviceAddress);
                return;
            }

            AndroidJavaClass uuidClass = new AndroidJavaClass("java.util.UUID");
            AndroidJavaObject sppUUID = uuidClass.CallStatic<AndroidJavaObject>("fromString", SPP_UUID);

            bluetoothSocket = device.Call<AndroidJavaObject>("createRfcommSocketToServiceRecord", sppUUID);
            bluetoothSocket.Call("connect");

            isConnected = true;
            StopScanUI();

            lastConnectedMAC = deviceAddress;
            lastConnectedName = FindDeviceNameByMAC(deviceAddress);

            PlayerPrefs.SetString("LastConnectedMAC", lastConnectedMAC);
            PlayerPrefs.SetString("LastConnectedName", lastConnectedName);
            PlayerPrefs.Save();

            Toast("Connected to " + FindDeviceNameByMAC(lastConnectedMAC));
            UpdateButtonSprite(true);
            UpdateConnectionStatus("Status: Connected to " + lastConnectedName);

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

    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        string disconnectedDeviceName = lastConnectedName;
        lastDisconnectedMAC = lastConnectedMAC;
        lastConnectedMAC = "";
        lastConnectedName = "";
        isConnected = false;

        UpdateConnectionStatus("Status: Disconnected");
        Toast("Disconnected from " + disconnectedDeviceName);

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

        UpdateButtonSprite(false);

        // Refresh both UI lists
        PopulateList(lastPairedList, pairedContentContainer, OnDeviceSelected);
        PopulateList(lastScannedList, scannedContentContainer, OnScannedDeviceSelected);

        if(extraPairedContentContainer != null)
            PopulateList(lastPairedList, extraPairedContentContainer, OnDeviceSelected);
        if(extraScannedContentContainer != null)
            PopulateList(lastScannedList, extraScannedContentContainer, OnScannedDeviceSelected);
    }

    // ------------------ Data Methods ------------------ //

    public void ReadData(string data)
    {
        Debug.Log("BT Stream: " + data);
        terminal?.LogReceived(data);
    }

    public void WriteData(string plainText)
    {
        if (Application.platform != RuntimePlatform.Android || !isConnected || string.IsNullOrWhiteSpace(plainText))
            return;

        try
        {
            if (bluetoothSocket == null)
            {
                Toast("Bluetooth socket is not connected.");
                return;
            }

            if (encryptionManager == null)
            {
                Toast("Encryption system not ready.");
                return;
            }

            string encryptedHex = encryptionManager.EncryptString(plainText);
            byte[] dataBytes = Encoding.UTF8.GetBytes(encryptedHex);
            byte[] finalBytes = new byte[dataBytes.Length + 1];
            Buffer.BlockCopy(dataBytes, 0, finalBytes, 0, dataBytes.Length);
            finalBytes[finalBytes.Length - 1] = (byte)'\n';

            AndroidJavaObject outputStream = bluetoothSocket.Call<AndroidJavaObject>("getOutputStream");
            outputStream.Call("write", finalBytes);
            outputStream.Call("flush");

            Toast($"Encrypted + Sent: {plainText} as {encryptedHex}");
            terminal?.LogSent(plainText);
        }
        catch (Exception ex)
        {
            Debug.LogError("Bluetooth Write Failed: " + ex);
            Toast("Write failed: " + ex.Message);
            StopConnection();
        }
    }

    // ------------------ UI Helpers ------------------ //

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
        if (container == null) return;

        if (container == pairedContentContainer || container == extraPairedContentContainer)
            lastPairedList = new List<string>(devices);
        else if (container == scannedContentContainer || container == extraScannedContentContainer)
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

            if (isSelected) comp.SetConnectionStatus("Connected", Color.green);
            else if (macAddress == lastDisconnectedMAC)
            {
                comp.SetConnectionStatus("Disconnected", Color.red);
                lastDisconnectedMAC = "";
            }
            else comp.SetConnectionStatus("");
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container);
        StartCoroutine(ResizeParentHeight(container));
    }

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
        if (container == null) return null;
        foreach (Transform child in container)
        {
            var entry = child.GetComponent<DeviceEntry>();
            if (entry != null && entry.MACAddress == mac)
                return entry;
        }
        return null;
    }
}
