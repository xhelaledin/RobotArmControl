# Unity Robot Arm Control

A Unity application for controlling a physical robot arm, including real-time 3D model visualization. The app sends commands to the robot arm and mirrors its movements in the 3D space, providing a seamless interface for testing and controlling your robotic system.

**Note:** This application will work as expected only if the robot arm hardware configuration and firmware code are compatible with the app's setup. Ensure that your robot arm uses the same configuration and code for communication to function properly.

## Features
- Real-time control of a physical robot arm
- 3D model simulation for precise movement visualization
- Bluetooth communication for wireless control
- Configurable arm joint angles
- Intuitive Unity UI for easy control
- Android support
- Save, view, run, and delete saved positions
- AES-128 encrypted Bluetooth communication with user-provided 16-character key (it should be same with the one in the microcontroller)

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
