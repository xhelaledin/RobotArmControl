using UnityEngine;

public class RobotArmInputHandler4Parts : MonoBehaviour
{
    public Transform part1, part2, part3;
    public Transform part4A, part4B, part5A, part5B;

    private bool[] directions = new bool[5];

    private float part1StartRotation;
    private float part2StartRotation;
    private float part3StartRotation;

    public void LoadStartRotationsFromPrefs()
    {
        part1StartRotation = PlayerPrefs.GetFloat("model4startRotationpart1", 0f);
        part2StartRotation = PlayerPrefs.GetFloat("model4startRotationpart2", 0f);
        part3StartRotation = PlayerPrefs.GetFloat("model4startRotationpart3", 0f);

        ApplyAllStartRotations();
    }

    private void ApplyAllStartRotations()
    {
        var adj1 = directions[0] ? part1StartRotation : -part1StartRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, adj1);

        var adj2 = directions[1] ? part2StartRotation : -part2StartRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, adj2);

        var adj3 = directions[2] ? part3StartRotation : -part3StartRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, adj3);
    }

    public void setPart1StartRotation(float zRotation)
    {
        part1StartRotation = zRotation;
        var adjusted = directions[0] ? zRotation : -zRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + adjusted);
    }

    public void setPart2StartRotation(float zRotation)
    {
        part2StartRotation = zRotation;
        var adjusted = directions[1] ? zRotation : -zRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 170 + adjusted);
    }

    public void setPart3StartRotation(float zRotation)
    {
        part3StartRotation = zRotation;
        var adjusted = directions[2] ? zRotation : -zRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 220 + adjusted);
    }

    public void SetDirection(int partIndex, bool isPositive)
    {
        if (partIndex >= 0 && partIndex < directions.Length)
            directions[partIndex] = isPositive;
    }

    public void setPart1Rotation(float rotation)
    {
        var adj = directions[0] ? rotation : -rotation;
        var angle = adj + part1StartRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + angle);
    }

    public void setPart2Rotation(float rotation)
    {
        var adj = directions[1] ? rotation : -rotation;
        var angle = adj + part2StartRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 80 + angle);
    }

    public void setPart3Rotation(float rotation)
    {
        var adj = directions[2] ? rotation : -rotation;
        var angle = adj + part3StartRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 20 + angle);
    }

    public void OpenClaw()
    {
        part4A.localEulerAngles = new Vector3(270f, 170f, 300f);
        part5A.localEulerAngles = new Vector3(90f, 190f, 62f);
        part4B.localEulerAngles = new Vector3(270f, 198f, 300f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 222f);
    }

    public void CloseClaw()
    {
        part4A.localEulerAngles = new Vector3(270f, 170f, 275f);
        part5A.localEulerAngles = new Vector3(90f, 190f, 50f);
        part4B.localEulerAngles = new Vector3(270f, 198f, 355f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 192f);
    }

    public void ApplySavedValues(int[] saveValues)
    {
        setPart1Rotation(saveValues[0]);
        setPart2Rotation(saveValues[1]);
        setPart3Rotation(saveValues[2]);

        if (saveValues.Length > 3 && saveValues[3] == 1)
            CloseClaw();
        else
            OpenClaw();
    }
}
