using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotArmInputHandler5Parts : MonoBehaviour
{
    public Transform part1, part2, part3, part4;
    public Transform part5A, part5B, part6A, part6B;

    [Header("Outline References")]
    public Outline outlinePart1;
    public Outline outlinePart2;
    public Outline outlinePart3;
    public Outline outlinePart4;
    public Outline outlinePart5A, outlinePart5B, outlinePart6A, outlinePart6B;

    private bool[] directions = new bool[5];

    private float part1StartRotation;
    private float part2StartRotation;
    private float part3StartRotation;
    private float part4StartRotation;

    [Header("Outline Settings")]
    public float outlineMaxWidth = 5f;
    public float outlineFadeDelay = 0.5f;
    public float outlineFadeDuration = 0.4f;

    private readonly Dictionary<Outline, Coroutine> fadeRoutines = new Dictionary<Outline, Coroutine>();

    private void Start()
    {
        LoadStartRotationsFromPrefs();
        InitAllOutlines();
    }

    private void OnDisable()
    {
        foreach (var kv in fadeRoutines)
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
        }
        fadeRoutines.Clear();
    }

    private void InitAllOutlines()
    {
        DisableOutline(outlinePart1);
        DisableOutline(outlinePart2);
        DisableOutline(outlinePart3);
        DisableOutline(outlinePart4);
        DisableOutline(outlinePart5A);
        DisableOutline(outlinePart5B);
        DisableOutline(outlinePart6A);
        DisableOutline(outlinePart6B);
    }

    private void DisableOutline(Outline outline)
    {
        if (outline != null)
        {
            outline.OutlineWidth = 0f;
            outline.enabled = false;
            CancelFade(outline);
        }
    }

    private void CancelFade(Outline outline)
    {
        if (outline == null) return;
        if (fadeRoutines.TryGetValue(outline, out var routine) && routine != null)
        {
            StopCoroutine(routine);
        }
        fadeRoutines.Remove(outline);
    }

    private void TriggerOutline(Outline outline)
    {
        if (outline == null) return;

        CancelFade(outline);

        outline.enabled = true;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = outlineMaxWidth;

        var co = StartCoroutine(FadeOutlineAfterInactivity(outline));
        fadeRoutines[outline] = co;
    }

    private IEnumerator FadeOutlineAfterInactivity(Outline outline)
    {
        yield return new WaitForSeconds(outlineFadeDelay);

        float elapsed = 0f;
        float startWidth = outline != null ? outline.OutlineWidth : 0f;

        while (outline != null && elapsed < outlineFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / outlineFadeDuration);
            outline.OutlineWidth = Mathf.Lerp(startWidth, 0f, t);
            yield return null;
        }

        if (outline != null)
        {
            outline.OutlineWidth = 0f;
            outline.enabled = false;
        }

        if (outline != null) fadeRoutines.Remove(outline);
    }

    // -------------------------
    // Rotation methods
    // -------------------------

    public void LoadStartRotationsFromPrefs()
    {
        part1StartRotation = PlayerPrefs.GetFloat("model5startRotationpart1", 0f);
        part2StartRotation = PlayerPrefs.GetFloat("model5startRotationpart2", 0f);
        part3StartRotation = PlayerPrefs.GetFloat("model5startRotationpart3", 0f);
        part4StartRotation = PlayerPrefs.GetFloat("model5startRotationpart4", 0f);

        setPart1StartRotation(part1StartRotation, 0);
        setPart2StartRotation(part2StartRotation, 0);
        setPart3StartRotation(part3StartRotation, 0);
        setPart4StartRotation(part4StartRotation, 0);
    }

    public void SetDirection(int partIndex, bool isPositive)
    {
        if (partIndex >= 0 && partIndex < directions.Length)
            directions[partIndex] = isPositive;
    }

    // Base (“start”) setters
    public void setPart1StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart1);

        part1StartRotation = zRotation;
        float adj = directions[0] ? zRotation : -zRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 270 + adj);
    }

    public void setPart2StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart2);

        part2StartRotation = zRotation;
        float adj = directions[1] ? zRotation : -zRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 105 + adj);
    }

    public void setPart3StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart3);

        part3StartRotation = zRotation;
        float adj = directions[2] ? zRotation : -zRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 250 + adj);
    }

    public void setPart4StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart4);

        part4StartRotation = zRotation;
        float adj = directions[3] ? zRotation : -zRotation;
        part4.localEulerAngles = new Vector3(270f, 129.6f, 320 + adj);
    }

    // Live (“delta”) rotators
    public void setPart1Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart1);

        float adj = directions[0] ? delta : -delta;
        float angle = adj + part1StartRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 270 + angle);
    }

    public void setPart2Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart2);

        float adj = directions[1] ? delta : -delta;
        float angle = adj + part2StartRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 105 + angle);
    }

    public void setPart3Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart3);

        float adj = directions[2] ? delta : -delta;
        float angle = adj + part3StartRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 250 + angle);
    }

    public void setPart4Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart4);

        float adj = directions[3] ? delta : -delta;
        float angle = adj + part4StartRotation;
        part4.localEulerAngles = new Vector3(270f, 129.6f, 320 + angle);
    }

    public void setPart1RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart1);

        float adj = directions[0] ? delta : -delta;
        part1.localEulerAngles = new Vector3(180f, 0f, 270 + adj);
    }

    public void setPart2RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart2);

        float adj = directions[1] ? delta : -delta;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 105 + adj);
    }

    public void setPart3RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart3);

        float adj = directions[2] ? delta : -delta;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 250 + adj);
    }

    public void setPart4RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart4);

        float adj = directions[3] ? delta : -delta;
        part4.localEulerAngles = new Vector3(270f, 129.6f, 320 + adj);
    }

    // Claw
    public void OpenClaw()
    {
        TriggerOutline(outlinePart5A);
        TriggerOutline(outlinePart5B);
        TriggerOutline(outlinePart6A);
        TriggerOutline(outlinePart6B);

        part5A.localEulerAngles = new Vector3(270f, 170f, 300f);
        part6A.localEulerAngles = new Vector3(90f, 190f, 62f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 300f);
        part6B.localEulerAngles = new Vector3(270f, 198f, 222f);
    }

    public void CloseClaw()
    {
        TriggerOutline(outlinePart5A);
        TriggerOutline(outlinePart5B);
        TriggerOutline(outlinePart6A);
        TriggerOutline(outlinePart6B);

        part5A.localEulerAngles = new Vector3(270f, 170f, 275f);
        part6A.localEulerAngles = new Vector3(90f, 190f, 50f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 355f);
        part6B.localEulerAngles = new Vector3(270f, 198f, 192f);
    }

    public void ApplySavedValues(int[] saveValues)
    {
        setPart1Rotation(saveValues[0], 0);
        setPart2Rotation(saveValues[1], 0);
        setPart3Rotation(saveValues[2], 0);
        setPart4Rotation(saveValues[3], 0);

        if (saveValues.Length > 4 && saveValues[4] == 1)
            CloseClaw();
        else
            OpenClaw();
    }
}
