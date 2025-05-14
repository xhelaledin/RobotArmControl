using UnityEngine;
using UnityEngine.SceneManagement; // Import SceneManager

public class RobotArmInputHandler : MonoBehaviour
{

    public Transform robotArmModel;

    // Sliders-controlled joints
    public Transform baseCylinder;
    public Transform joint1;
    public Transform joint2;

    // Button-controlled joints
    public Transform joint3R;
    public Transform joint4R;
    public Transform joint3L;
    public Transform joint4L;

    // Method to set the base rotation (controlled by slider)
    public void SetBaseRotation(float zRotation)
    {
        Debug.Log($"Setting Base Rotation: {zRotation}");
        baseCylinder.localEulerAngles = new Vector3(180f, 0f, -zRotation*1.8f);
    }

    // Method to set Joint1 rotation (controlled by slider)
    public void SetJoint1Rotation(float yRotation)
    {
        Debug.Log($"Setting Joint1 Rotation: {yRotation}");
        joint1.localRotation = Quaternion.Euler(270f, -yRotation+80, 0.185f);
    }

    // Method to set Joint2 rotation (controlled by slider)
    public void SetJoint2Rotation(float yRotation)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name == "RobotArmScene")
    {
        if (yRotation > 41 )
        {
            robotArmModel.position = new Vector3(2.74f, 3.1f, -40.6f);
        }
        else
        {
            robotArmModel.position = new Vector3(2.8f, 1.38f, -48f);
        }
    }
    else if (activeScene.name == "SavedPositionsScene")
    {
        if (yRotation < 94)
        {
            robotArmModel.position = new Vector3(2.8f, 5.6f, 14f);
        }
        else
        {
            robotArmModel.position = new Vector3(3.1f, 4.6f, 5.8f);
        }
    }
        Debug.Log($"Setting Joint2 Rotation: {yRotation}");
        joint2.localRotation = Quaternion.Euler(270f, -yRotation+40, 129.6f);
    }

    // Method for "Set Button" to set specific positions for Joint3R and Joint3L
    public void SetJoints3Position()
    {
        joint3R.localRotation = Quaternion.Euler(-90f, 18f, 179.767f);
        joint4R.localRotation = Quaternion.Euler(-90f, -23f, 44.603f);
        joint3L.localRotation = Quaternion.Euler(90f, -24f, 177.905f);
        joint4L.localRotation = Quaternion.Euler(-90f, -20f, 44.603f);
    }

    // Method for "Close Button" to reset Joint3R and Joint3L to original positions
    public void ResetJoints3Position()
    {
        joint3R.localRotation = Quaternion.Euler(-90f, 0f, 179.767f);
        joint4R.localRotation = Quaternion.Euler(-90f, 0f, 44.603f);
        joint3L.localRotation = Quaternion.Euler(90f, 0f, 177.905f);
        joint4L.localRotation = Quaternion.Euler(-90f, 0f, 44.603f);
    }
}
