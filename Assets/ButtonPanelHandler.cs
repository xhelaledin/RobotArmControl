using UnityEngine;
using UnityEngine.UI;

public class ButtonPanelHandler : MonoBehaviour
{
    // Main Buttons
    public Button bluetoothButton;           // The main button (BluetoothButton)
    public Button savePositionButton;        // The main button (SavePositionButton)
    public Button terminalButton;
    public Button logButton;
    public Button keyButton;
    
    // Secondary Buttons
    public Button bluetoothButton2;          // The second button (BluetoothButton2)
    public Button savePositionButton2;       // The second button (SavePositionButton2)
    public Button terminalButton2;
    public Button logButton2;
    public Button keyButton2;

    // Close Buttons
    public Button bluetoothCloseButton;      // The close button for BluetoothPanel
    public Button savePositionCloseButton;   // The close button for SavePositionPanel
    public Button savePositionSendButton;
    public Button terminalCloseButton; 
    public Button terminalSendButton;
    public Button logCloseButton;
    public Button keyCloseButton;

    public Button keyCloseSetButton;
    

    private void Start()
    {
        // Initially hide the second buttons
        bluetoothButton2.gameObject.SetActive(false);
        savePositionButton2.gameObject.SetActive(false);
        terminalButton2.gameObject.SetActive(false);
        logButton2.gameObject.SetActive(false);
        keyButton2.gameObject.SetActive(false);

        // Add listeners for main buttons and close buttons
        bluetoothButton.onClick.AddListener(ShowBluetoothButton2);
        savePositionButton.onClick.AddListener(ShowSavePositionButton2);
        terminalButton.onClick.AddListener(ShowTerminalButton2);
        logButton.onClick.AddListener(ShowLogButton2);
        keyButton.onClick.AddListener(ShowKeyButton2);

        bluetoothCloseButton.onClick.AddListener(HideBluetoothButton2);
        savePositionCloseButton.onClick.AddListener(HideSavePositionButton2);
        savePositionSendButton.onClick.AddListener(HideSavePositionButton2);
        terminalCloseButton.onClick.AddListener(HideTerminalButton2);
        terminalSendButton.onClick.AddListener(HideTerminalButton2);
        logCloseButton.onClick.AddListener(HideLogButton2);
        keyCloseButton.onClick.AddListener(HideKeyButton2);
        keyCloseSetButton.onClick.AddListener(HideKeyButton2);
    }

    // This method will be called when the BluetoothButton is clicked
    private void ShowBluetoothButton2()
    {
        bluetoothButton2.gameObject.SetActive(true); // Show BluetoothButton2
    }

    // This method will be called when the SavePositionButton is clicked
    private void ShowSavePositionButton2()
    {
        savePositionButton2.gameObject.SetActive(true); // Show SavePositionButton2
    }

     private void ShowTerminalButton2()
    {
        terminalButton2.gameObject.SetActive(true); // Show SavePositionButton2
    }

      private void ShowLogButton2()
    {
        logButton2.gameObject.SetActive(true); // Show SavePositionButton2
    }

    private void ShowKeyButton2()
    {
        keyButton2.gameObject.SetActive(true); // Show SavePositionButton2
    }

    // This method will be called when the Bluetooth close button is clicked
    private void HideBluetoothButton2()
    {
        bluetoothButton2.gameObject.SetActive(false); // Hide BluetoothButton2
    }

    // This method will be called when the SavePosition close button is clicked
    private void HideSavePositionButton2()
    {
        savePositionButton2.gameObject.SetActive(false); // Hide SavePositionButton2
    }


    private void HideTerminalButton2()
    {
        terminalButton2.gameObject.SetActive(false); // Hide SavePositionButton2
    }


    private void HideLogButton2()
    {
        logButton2.gameObject.SetActive(false); // Hide SavePositionButton2
    }

    private void HideKeyButton2()
    {
        keyButton2.gameObject.SetActive(false); // Hide SavePositionButton2
    }
    
}
