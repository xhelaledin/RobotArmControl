using UnityEngine;

public class RobotArmInputHandler5BParts : MonoBehaviour
{
    public Transform part1, part2, part3, part4;
    public Transform part5A, part5B, part6A, part6B;

    private bool[] directions = new bool[5];
    private float part1StartRotation;
    private float part2StartRotation;
    private float part3StartRotation;
    private float part4StartRotation;

    private void Start()
    {
        LoadStartRotationsFromPrefs();
    }

    public void LoadStartRotationsFromPrefs()
    {
        part1StartRotation = PlayerPrefs.GetFloat("model5BstartRotationpart1", 0f);
        part2StartRotation = PlayerPrefs.GetFloat("model5BstartRotationpart2", 0f);
        part3StartRotation = PlayerPrefs.GetFloat("model5BstartRotationpart3", 0f);
        part4StartRotation = PlayerPrefs.GetFloat("model5BstartRotationpart4", 0f);

        setPart1StartRotation(part1StartRotation);
        setPart2StartRotation(part2StartRotation);
        setPart3StartRotation(part3StartRotation);
        setPart4StartRotation(part4StartRotation);
    }

    public void SetDirection(int partIndex, bool isPositive)
    {
        if (partIndex >= 0 && partIndex < directions.Length)
            directions[partIndex] = isPositive;
    }

    // Base rotations
    public void setPart1StartRotation(float zRotation)
    {
        part1StartRotation = zRotation;
        float adj = directions[0] ? zRotation : -zRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 270 + adj);
    }

    public void setPart2StartRotation(float zRotation)
    {
        part2StartRotation = zRotation;
        float adj = directions[1] ? zRotation : -zRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 260 + adj);
    }

    public void setPart3StartRotation(float zRotation)
    {
        part3StartRotation = zRotation;
        float adj = directions[2] ? zRotation : -zRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 310 + adj);
    }

    public void setPart4StartRotation(float zRotation)
    {
        part4StartRotation = zRotation;
        float adj = directions[3] ? zRotation : -zRotation;
        part4.localEulerAngles = new Vector3(0f, 0f, adj);
    }

    // Live updates
    public void setPart1Rotation(float delta)
    {
        float adj = directions[0] ? delta : -delta;
        float angle = adj + part1StartRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 270 + angle);
    }

    public void setPart2Rotation(float delta)
    {
        float adj = directions[1] ? delta : -delta;
        float angle = adj + part2StartRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 260 + angle);
    }

    public void setPart3Rotation(float delta)
    {
        float adj = directions[2] ? delta : -delta;
        float angle = adj + part3StartRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 310 + angle);
    }

    public void setPart4Rotation(float delta)
    {
        float adj = directions[3] ? delta : -delta;
        float angle = adj + part4StartRotation;
        part4.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    // Claw logic
    public void OpenClaw()
    {
        part5A.localEulerAngles = new Vector3(270f, 170f, 300f);
        part6A.localEulerAngles = new Vector3(90f, 190f, 62f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 300f);
        part6B.localEulerAngles = new Vector3(270f, 198f, 222f);
    }

    public void CloseClaw()
    {
        part5A.localEulerAngles = new Vector3(270f, 170f, 275f);
        part6A.localEulerAngles = new Vector3(90f, 190f, 50f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 355f);
        part6B.localEulerAngles = new Vector3(270f, 198f, 230f);
    }

    public void ApplySavedValues(int[] saveValues)
    {
        setPart1Rotation(saveValues[0]);
        setPart2Rotation(saveValues[1]);
        setPart3Rotation(saveValues[2]);
        setPart4Rotation(saveValues[3]);

        if (saveValues.Length > 4 && saveValues[4] == 1)
            CloseClaw();
        else
            OpenClaw();
    }
}
