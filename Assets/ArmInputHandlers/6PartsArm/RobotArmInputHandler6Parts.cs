using UnityEngine;

public class RobotArmInputHandler6Parts : MonoBehaviour
{
    public Transform part1, part2, part3, part4, part5;
    public Transform part6A, part6B, part7A, part7B;

    private bool[] directions = new bool[5];
    private float part1StartRotation;
    private float part2StartRotation;
    private float part3StartRotation;
    private float part4StartRotation;
    private float part5StartRotation;

    private void Start()
    {
        LoadStartRotationsFromPrefs();
    }

    public void LoadStartRotationsFromPrefs()
    {
        part1StartRotation = PlayerPrefs.GetFloat("model6startRotationpart1", 0f);
        part2StartRotation = PlayerPrefs.GetFloat("model6startRotationpart2", 0f);
        part3StartRotation = PlayerPrefs.GetFloat("model6startRotationpart3", 0f);
        part4StartRotation = PlayerPrefs.GetFloat("model6startRotationpart4", 0f);
        part5StartRotation = PlayerPrefs.GetFloat("model6startRotationpart5", 0f);

        setPart1StartRotation(part1StartRotation);
        setPart2StartRotation(part2StartRotation);
        setPart3StartRotation(part3StartRotation);
        setPart4StartRotation(part4StartRotation);
        setPart5StartRotation(part5StartRotation);
    }

    public void SetDirection(int partIndex, bool isPositive)
    {
        if (partIndex >= 0 && partIndex < directions.Length)
            directions[partIndex] = isPositive;
    }

    public void setPart1StartRotation(float zRotation)
    {
        part1StartRotation = zRotation;
        float adj = directions[0] ? zRotation : -zRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + adj);
    }

    public void setPart2StartRotation(float zRotation)
    {
        part2StartRotation = zRotation;
        float adj = directions[1] ? zRotation : -zRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 290 + adj);
    }

    public void setPart3StartRotation(float zRotation)
    {
        part3StartRotation = zRotation;
        float adj = directions[2] ? zRotation : -zRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 240 + adj);
    }

    public void setPart4StartRotation(float zRotation)
    {
        part4StartRotation = zRotation;
        float adj = directions[3] ? zRotation : -zRotation;
        part4.localEulerAngles = new Vector3(270f, 129.6f, 340 + adj);
    }

    public void setPart5StartRotation(float zRotation)
    {
        part5StartRotation = zRotation;
        float adj = directions[4] ? zRotation : -zRotation;
        part5.localEulerAngles = new Vector3(0f, 0f, adj);
    }

    public void setPart1Rotation(float delta)
    {
        float adj = directions[0] ? delta : -delta;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + adj + part1StartRotation);
    }

    public void setPart2Rotation(float delta)
    {
        float adj = directions[1] ? delta : -delta;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 290 + adj + part2StartRotation);
    }

    public void setPart3Rotation(float delta)
    {
        float adj = directions[2] ? delta : -delta;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 240 + adj + part3StartRotation);
    }

    public void setPart4Rotation(float delta)
    {
        float adj = directions[3] ? delta : -delta;
        part4.localEulerAngles = new Vector3(270f, 129.6f, 340 + adj + part4StartRotation);
    }

    public void setPart5Rotation(float delta)
    {
        float adj = directions[4] ? delta : -delta;
        part5.localEulerAngles = new Vector3(0f, 0f, adj + part5StartRotation);
    }

    public void OpenClaw()
    {
        part6A.localEulerAngles = new Vector3(270f, 170f, 300f);
        part7A.localEulerAngles = new Vector3(90f, 190f, 62f);
        part6B.localEulerAngles = new Vector3(270f, 198f, 300f);
        part7B.localEulerAngles = new Vector3(270f, 198f, 222f);
    }

    public void CloseClaw()
    {
        part6A.localEulerAngles = new Vector3(270f, 170f, 275f);
        part7A.localEulerAngles = new Vector3(90f, 190f, 50f);
        part6B.localEulerAngles = new Vector3(270f, 198f, 355f);
        part7B.localEulerAngles = new Vector3(270f, 198f, 192f);
    }

    public void ApplySavedValues(int[] saveValues)
    {
        setPart1Rotation(saveValues[0]);
        setPart2Rotation(saveValues[1]);
        setPart3Rotation(saveValues[2]);
        setPart4Rotation(saveValues[3]);
        setPart5Rotation(saveValues[4]);

        if (saveValues.Length > 5 && saveValues[5] == 1)
            CloseClaw();
        else
            OpenClaw();
    }
}
