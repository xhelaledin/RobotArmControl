# Robot Arm Control App

A professional Unity-based Android application for controlling physical robot arms. This app acts as a digital twin, allowing you to control your hardware via **Bluetooth Serial** while visualizing movements in real-time on a 3D model.

## 📥 Download App
**[Download the latest APK (v1.0.0) here](https://github.com/xhelaledin/RobotArmControl/releases/latest)**

---

## ✨ Key Features

### 🎮 Control & Visualization
* **Real-time Digital Twin:** The 3D model mirrors your physical robot instantly.
* **Smart UI:** The interface automatically adapts to your hardware. When you touch a slider, the corresponding part on the 3D model highlights with an outline so you know exactly what you are moving.
* **Multi-Model Support:** The app supports 4 distinct robot configurations. The sliders change automatically based on your selection:
    * **4-Axis** (3 Sliders)
    * **5-Axis** (4 Sliders)
    * **5b-Axis** (4 Sliders)
    * **6-Axis** (5 Sliders)

### 🤖 Automation & Saves
* **SaveLists:** Create lists of saved positions to build automation routines.
* **Custom Start:** Configure specific visual start positions for the 3D model.
* **Data Isolation:** Each model type (e.g., 4-Axis vs 6-Axis) has its own separate storage for saves, savelists, and visual settings.

### 🔒 Security & Connectivity
* **Bluetooth Serial:** Uses standard Bluetooth Serial Port Profile (SPP) for wide compatibility with HC-05, ESP32, and Arduino Bluetooth modules.
* **Flexible Encryption:** Choose the security level that fits your project:
    1.  **None:** Standard raw communication.
    2.  **AES-128:** High-security encryption (Requires 16-character key).
    3.  **DES:** Standard encryption (Requires key).
* **Code Helper:** The app provides ready-to-use Arduino code snippets for all 3 encryption modes.

### 💾 Backup & Restore
* **JSON System:** Deeply integrated Import/Export system.
    * **Settings:** Backup visual and app settings by category.
    * **Saves:** Backup your position libraries to JSON files. Note that importing saves replaces the current list.

---

## 📱 Installation Guide

1.  Download the `.apk` file from the **[Releases Page](https://github.com/xhelaledin/RobotArmControl/releases)**.
2.  Transfer the file to your Android device (or download it directly on the device).
3.  Tap the file to install.
    * *Note: You may need to allow "Install from Unknown Sources" in your Android settings if this is your first time installing a GitHub APK.*
4.  Open the app and grant the required Bluetooth permissions.

---

## 🛠 Hardware Setup & Connection

To use this app, your robot arm needs a microcontroller (like Arduino or ESP32) with a Bluetooth Serial module.

### 1. Firmware Setup
* Open the App and navigate to the **Code Examples** section.
* Select your preferred encryption mode (None, AES, or DES).
* Copy the example code provided in the app and upload it to your microcontroller.

### 2. Encryption Keys (Critical)
If you select **AES** or **DES** encryption, security is paramount:
* You must define a secret Key in your Arduino code.
* **IMPORTANT:** You must enter **exactly the same Key** into the App's encryption settings field. If the keys do not match, the robot will not understand the commands.

### 3. Connecting
1.  Pair your Android phone with your Bluetooth module (e.g., HC-05) via your phone's Android Bluetooth settings.
2.  Open the App.
3.  Select your Robot Model.
4.  Connect to the device from the list.

---

## 👨‍💻 For Developers

If you wish to modify the source code or build the project yourself:

### Prerequisites
* Unity 2021.3 or newer
* Android Build Support module

### Building from Source
1.  Clone this repository:
    ```bash
    git clone [https://github.com/xhelaledin/RobotArmControl.git](https://github.com/xhelaledin/RobotArmControl.git)
    ```
2.  Open the project in Unity.
3.  Import your specific robot arm models (FBX/OBJ) if different from the defaults.
4.  Build specifically for the **Android** platform.

---

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Contact
For support or inquiries, please reach out at [contact@xhelaledin.com].
