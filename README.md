# Unity Robot Arm Control

A Unity application for controlling a physical robot arm, including real-time 3D model visualization. The app sends commands to the robot arm and mirrors its movements in the 3D space, providing a seamless interface for testing and controlling your robotic system.

**Note:** This application will work as expected only if the robot arm hardware configuration and firmware code are compatible with the app's setup. Ensure that your robot arm uses the same configuration and code for communication to function properly.

## Features
- Real-time control of a physical robot arm
- 3D model simulation for precise movement visualization
- Bluetooth communication for wireless control
- Configurable arm joint angles
- Intuitive Unity UI for easy control
- Android support only
- Save, view, run, and delete saved positions
- AES-128 encrypted Bluetooth communication with user-provided 16-character key

## Prerequisites
- Unity 2021.3 or newer
- Robot arm with Bluetooth capability (e.g., ESP32 or Arduino-based)
- BluetoothManager script for Unity
- 3D model of the robot arm (FBX or OBJ format)
- TextMesh Pro (for UI elements)

## Installation
1. Clone this repository:
   ```bash
   git clone https://github.com/xhelaledin/RobotArmControl.git
   cd RobotArmControl
   ```
2. Open the project in Unity.
3. Import the required 3D model for the robot arm.
4. Ensure the BluetoothManager script is properly configured.
5. Connect your physical robot arm via Bluetooth.

## Releases
You can download the latest release from the [Releases page](https://github.com/xhelaledin/RobotArmControl/releases).

## Usage
- Use the on-screen controls to send commands to the robot arm.
- The 3D model will update in real-time to match the physical arm's position.
- Adjust joint angles through sliders or input fields as needed.
- Save and manage positions using the built-in position manager.

## Communication Protocol
- The app sends encrypted commands via AES-128 with a 16-character key provided by the user.
- - The key should be the same with the one present in the microcontroller.
- Ensure the robot firmware is compatible with the app's command structure.

## Contributing
Feel free to open issues or submit pull requests to improve this project.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Contact
For support or inquiries, please reach out at [xhelaledin@gmail.com].
