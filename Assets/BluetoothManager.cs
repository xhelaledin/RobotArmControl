using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro; // For TextMeshPro components
using System.Text; // For Encoding
using UnityEngine.SceneManagement;
using System;


public class BluetoothManager : MonoBehaviour
{

        
    // UI elements (all use TextMeshProUGUI)
    public TextMeshProUGUI dataToSend; // Data to be sent
    public TextMeshProUGUI receivedData; // Incoming data display
    public TextMeshProUGUI connectionStatus; // Bluetooth status
    public TextMeshProUGUI scanningStatusText; // Scanning Status
    public TMP_InputField inputFieldToSend;  // Reference to the TMP_InputField inside the panel
    public GameObject bluetoothMainPanel; // Main panel that opens from the main page (contains the Connect button)
    public GameObject sendPanel;  // Reference to the send panel
    public GameObject bluetoothDevicePanel; // Panel that displays the list of paired devices
    public GameObject bluetoothScanPanel; // Single UI panel for scanning results (USED NOW)
    public GameObject devicesListContainer; // Container (Content of Scroll View) for paired devices list
    public GameObject scannedDevicesListContainer; // Container (Content of Scroll View) for scanned devices list
    public GameObject logPanel; // Assign the UI panel in Inspector
    public GameObject deviceMACTextPrefab; // Prefab for each paired/scanned device button

    // To store MAC addresses dynamically
    private Dictionary<string, GameObject> scannedDevices = new Dictionary<string, GameObject>();


    // Bluetooth connection status and selected device information
    private bool isConnected = false;

    // Android API objects
    private AndroidJavaObject bluetoothAdapter; // Android BluetoothAdapter instance
    private AndroidJavaObject bluetoothSocket; // BluetoothSocket for the connection

    private static AndroidJavaClass unity3dbluetoothplugin;
    private static AndroidJavaObject BluetoothConnector;

    // Standard SPP UUID (Bluetooth Serial Port Profile)
    private string SPP_UUID = "00001101-0000-1000-8000-00805f9b34fb";

    public AesEncryptor aesEncryptor; // Assign in Inspector

    public BluetoothLoggerUI logger; // Assign in Inspector

    void Start()
    {
        // Initialize the Bluetooth adapter.
        InitBluetooth();

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name == "SavedPositionsScene") {
            string savedMAC = PlayerPrefs.GetString("LastConnectedMAC", "");
            if (!string.IsNullOrEmpty(savedMAC))
            {
                StartConnection(savedMAC); // Your function for connecting via Bluetooth
            }
        }
    }

    /// <summary>
    /// Initializes the Bluetooth adapter and requests any needed permissions.
    /// </summary>
    public void InitBluetooth()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        // Request necessary permissions.
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

        // Obtain the default BluetoothAdapter using Android's API.
        AndroidJavaClass bluetoothAdapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter");
        bluetoothAdapter = bluetoothAdapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");

        unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
        BluetoothConnector = unity3dbluetoothplugin.CallStatic<AndroidJavaObject>("getInstance");

        if (bluetoothAdapter == null)
        {
            Toast("Bluetooth not supported on this device");
            return;
        }

        //Toast("Bluetooth Ready");
        UpdateConnectionStatus("Status: Disconnected");
    }

    /// <summary>
    /// Opens the main Bluetooth panel.
    /// (Hook this up to your main page Bluetooth button.)
    /// </summary>
    public void ShowBluetoothPanel()
    {
        if (bluetoothMainPanel != null)
        {
            bluetoothMainPanel.SetActive(true);
        }
        else
        {
            Toast("Bluetooth main panel not set in Inspector.");
        }
    }

    public void HideBluetoothPanel()
    {
        if (bluetoothScanPanel != null)
        {
            bluetoothMainPanel.SetActive(false);
        }
    }
    

    /// <summary>
    /// Called when the Connect button inside the main panel is pressed.
    /// Opens the device list panel.
    /// (Hook this function to the Connect button’s OnClick event.)
    /// </summary>
    public void OnConnectButtonPressed()
    {
        ShowDeviceListPanel();
    }

    /// <summary>
    /// Activates the paired devices panel and retrieves paired devices.
    /// </summary>
    public void ShowDeviceListPanel()
    {
        if (bluetoothDevicePanel != null)
        {
            bluetoothDevicePanel.SetActive(true);
            GetPairedDevices();
        }
        else
        {
            Toast("Bluetooth device panel not set in Inspector.");
        }
    }

    public void HideDeviceListPanel()
    {
        if (bluetoothScanPanel != null)
        {
            bluetoothDevicePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Retrieves the paired devices, dynamically instantiates a list of buttons for each device, and displays them.
    /// </summary>
    public void GetPairedDevices()
{
    if (Application.platform != RuntimePlatform.Android) return;

    // Retrieve the set of paired devices
    AndroidJavaObject pairedDevices = bluetoothAdapter.Call<AndroidJavaObject>("getBondedDevices");

    if (pairedDevices == null)
    {
        Toast("No paired devices found.");
        Debug.LogError("No paired devices found.");
        return;
    }

    // Log the number of paired devices
    int deviceCount = pairedDevices.Call<int>("size");
    Debug.Log("Paired devices count: " + deviceCount);

    if (deviceCount == 0)
    {
        Toast("No paired devices found.");
        Debug.Log("No paired devices found.");
        return;
    }

    // Clear the paired devices list container (if any previous items are there)
    foreach (Transform child in devicesListContainer.transform)
    {
        Destroy(child.gameObject);
    }

    // Iterate over the paired devices
    AndroidJavaObject iterator = pairedDevices.Call<AndroidJavaObject>("iterator");
    while (iterator.Call<bool>("hasNext"))
    {
        AndroidJavaObject device = iterator.Call<AndroidJavaObject>("next");

        // Get device name and address
        string deviceName = device.Call<string>("getName");
        string deviceAddress = device.Call<string>("getAddress");

        // Log the device information to help debug
        Debug.Log("Paired Device - Name: " + deviceName + ", Address: " + deviceAddress);

        string entry = deviceName + "\n" + deviceAddress;

        // Instantiate a new device entry using the prefab
        GameObject newDeviceGO = Instantiate(deviceMACTextPrefab, devicesListContainer.transform);
        TextMeshProUGUI deviceText = newDeviceGO.GetComponentInChildren<TextMeshProUGUI>();
        if (deviceText != null) deviceText.text = entry;

        Button deviceButton = newDeviceGO.GetComponent<Button>();
        if (deviceButton != null)
        {
            // Capture the device address for use in the listener
            string addr = deviceAddress;
            deviceButton.onClick.AddListener(() =>
            {
                OnDeviceSelected(addr);
            });
        }
    }
}


    /// <summary>
    /// Called when a device from the list is selected.
    /// Attempts to connect to the device and, if successful, hides the device list panel.
    /// If the connection fails, the device list stays visible so the user can select another device.
    /// </summary>
    public void OnDeviceSelected(string address)
    {
        StartConnection(address);

        // Only remove the list (close the panel) if connection is successful.
        if (isConnected && bluetoothDevicePanel != null)
        {
            bluetoothDevicePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Initiates a connection to the selected device.
    /// </summary>
    public void StartConnection(string deviceAddress)
    {
        if (Application.platform != RuntimePlatform.Android) return;
        if (string.IsNullOrEmpty(deviceAddress))
        {
            Toast("No device address provided.");
            return;
        }

        try
        {
            // Get the remote Bluetooth device.
            AndroidJavaObject device = bluetoothAdapter.Call<AndroidJavaObject>("getRemoteDevice", deviceAddress);
            if (device == null)
            {
                Toast("Failed to get device with address: " + deviceAddress);
                return;
            }

            // Get the SPP UUID from Java's UUID.fromString(String) method.
            AndroidJavaClass uuidClass = new AndroidJavaClass("java.util.UUID");
            AndroidJavaObject sppUUID = uuidClass.CallStatic<AndroidJavaObject>("fromString", SPP_UUID);

            // Create an RFCOMM socket.
            bluetoothSocket = device.Call<AndroidJavaObject>("createRfcommSocketToServiceRecord", sppUUID);
            bluetoothSocket.Call("connect");

            isConnected = true;
            Toast("Connected to " + deviceAddress);
            UpdateConnectionStatus("Status: Connected to " + deviceAddress);
        }
        catch (System.Exception ex)
        {
            // Connection failed.
            isConnected = false;
            Toast("Connection failed: " + ex.Message);
            UpdateConnectionStatus("Status: Connection failed");
        }
        SaveDeviceMAC(deviceAddress);
    }

    /// <summary>
    /// Writes data from the dataToSend TMP text component to the connected device.
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
        // Encrypt the string
        string encryptedHex = aesEncryptor.EncryptString(plainText);

        // Convert encrypted hex to bytes
        byte[] dataBytes = Encoding.UTF8.GetBytes(encryptedHex);

        // Append newline to indicate end of message
        byte[] final = new byte[dataBytes.Length + 1];
        Buffer.BlockCopy(dataBytes, 0, final, 0, dataBytes.Length);
        final[final.Length - 1] = (byte)'\n';

        // Write to Bluetooth output stream
        AndroidJavaObject outputStream = bluetoothSocket.Call<AndroidJavaObject>("getOutputStream");
        outputStream.Call("write", final);
        outputStream.Call("flush");

        Toast("Encrypted + Sent: " + plainText + " as " + encryptedHex);

        if (logger != null)
        {
            logger.LogMessage($"Sent: {plainText}");
            Debug.Log("[BluetoothManager] Logger is assigned and message logged.");
        }
        else
        {
            Debug.LogWarning("[BluetoothManager] Logger is null. Message not logged.");
        }

    }
    catch (Exception ex)
    {
        Debug.LogError("Bluetooth Write Failed: " + ex);
        Toast("Write failed: " + ex.Message);
    }
}




    /// <summary>
    /// Closes the Bluetooth connection.
    /// </summary>
    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        if (bluetoothSocket != null)
        {
            try
            {
                bluetoothSocket.Call("close");
                bluetoothSocket = null;
                isConnected = false;
                Toast("Disconnected");
                UpdateConnectionStatus("Status: Disconnected");
            }
            catch (System.Exception ex)
            {
                Toast("Disconnect failed: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Displays a Toast using Android's Toast API.
    /// </summary>
    public void Toast(string message)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("Toast: " + message);
            return;
        }

        // Run on UI thread.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, 0);
            toastObject.Call("show");
        }));
    }

    /// <summary>
    /// Updates the connection status TMP text.
    /// </summary>
    public void UpdateConnectionStatus(string status)
    {
        if (connectionStatus != null)
            connectionStatus.text = status;
    }

    /// <summary>
    /// Opens the scan panel and starts scanning for nearby devices.
    /// </summary>
    public void OnScanButtonPressed()
    {
        if (bluetoothScanPanel != null)
        {
            bluetoothScanPanel.SetActive(true);
            StartScan();
        }
        else
        {
            Toast("Bluetooth scan panel not set in Inspector.");
        }
    }

    // Called when user presses "Start Scan Again" button
    public void OnStartScanButtonPressed()
    {
        StartScan();
    }

    // Start scanning devices
    public void StartScan()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // Clear the scanned devices list container to update the list
        foreach (Transform child in scannedDevicesListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        BluetoothConnector.CallStatic("StartScanDevices");
    }

    // Called from JAVA when scan status changes
    public void ScanStatus(string status)
    {
        Toast("Scan Status: " + status);

        if (status == "stopped" || status == "completed")
        {
            if (scanningStatusText != null)
                scanningStatusText.text = "Scan Stopped";
        }
    }

    /// <summary>
    /// Called by the Java side when a new device is found during scan.
    /// </summary>
    public void NewDeviceFound(string data)
    {
        Debug.Log("New device found: " + data);  // Debug log to verify devices are being passed.
        if (string.IsNullOrEmpty(data)) return;  // Ensure data is valid before proceeding.

        if (scannedDevices.ContainsKey(data)) return;  // Prevent adding duplicate devices

        // Instantiate a new device UI element from the prefab
        GameObject newDeviceGO = Instantiate(deviceMACTextPrefab, scannedDevicesListContainer.transform);
        TextMeshProUGUI deviceText = newDeviceGO.GetComponentInChildren<TextMeshProUGUI>();

        if (deviceText != null) deviceText.text = data;  // Set the text to the device info

        Button deviceButton = newDeviceGO.GetComponent<Button>();
        if (deviceButton != null)
        {
            // Extract the MAC address from the data (assuming data is in the format "MAC+NAME")
            string address = data.Split('+')[0];  // Extracting only the MAC address part
            deviceButton.onClick.AddListener(() =>
            {
                // Handle device selection
                OnScannedDeviceSelected(address);
            });
        }

        // Store the device entry in the dictionary to avoid duplicates
        scannedDevices[data] = newDeviceGO;
    }


    /// <summary>
    /// Called when a user selects a scanned device from the scan results.
    /// </summary>
    public void OnScannedDeviceSelected(string address)
    {
        StartConnection(address);

        // Optionally close the scan panel
        if (isConnected && bluetoothScanPanel != null)
        {
            bluetoothScanPanel.SetActive(false);
        }
    }

    public void HideScanPanel()
    {
        if (bluetoothScanPanel != null)
        {
            bluetoothScanPanel.SetActive(false);
            StopScanDevices(); // optional: also stop scanning when closing
        }
    }

    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        // Stop scanning devices directly here
        AndroidJavaClass bluetoothClass = new AndroidJavaClass("com.example.bluetooth.BluetoothManager");
        bluetoothClass.CallStatic("StopScanDevices");
    }

    // Show the send panel
    public void ShowSendPanel()
    {
        sendPanel.SetActive(true);  // Show the panel
    }

    // Hide the send panel
    public void HideSendPanel()
    {
        sendPanel.SetActive(false);  // Hide the panel
    }

    // Send the data and close the panel
    public void SendData()
    {
        string data = inputFieldToSend.text;

            // Check if the data is null or empty
        if (string.IsNullOrEmpty(data))
        {
            Toast("Cannot send empty or null data!");  // Show a toast message
            return;  // Exit the function early if the data is invalid
        }

        WriteData(data);  // Call WriteData with the text from the input field
        Toast("Message Sent: " + data);

        // Clear the input field after sending the data
        inputFieldToSend.text = ""; 
    }

     // Cancel the sending and close the panel
    public void CancelSend()
    {
        inputFieldToSend.text = "";  // Clear the input field
        HideSendPanel();  // Close the panel
    }

    

    void SaveDeviceMAC(string macAddress)
    {
        PlayerPrefs.SetString("LastConnectedMAC", macAddress);
        PlayerPrefs.Save();
    }


     public void ShowLogPanel()
    {
        logPanel.SetActive(true);
    }

    public void HideLogPanel()
    {
        logPanel.SetActive(false);
    }

}
