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
    // Rotation helpers
    // -------------------------

    private float AdjustedAngle(float rotation, float offset, bool reversed)
    {
        float angle = reversed ? offset + (360f - rotation) : offset + rotation;
        return angle % 360f;
    }

    // -------------------------
    // Start rotations
    // -------------------------

    public void LoadStartRotationsFromPrefs()
    {
        // USING REGISTRY FOR DEFAULTS
        part1StartRotation = PlayerPrefsKeyRegistry.GetFloat("model5startRotationpart1");
        part2StartRotation = PlayerPrefsKeyRegistry.GetFloat("model5startRotationpart2");
        part3StartRotation = PlayerPrefsKeyRegistry.GetFloat("model5startRotationpart3");
        part4StartRotation = PlayerPrefsKeyRegistry.GetFloat("model5startRotationpart4");

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

    public void setPart1StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart1);
        part1StartRotation = zRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, AdjustedAngle(0f, 95f, directions[0]));
    }

    public void setPart2StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart2);
        part2StartRotation = zRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, AdjustedAngle(0f, 290f, directions[1]));
    }

    public void setPart3StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart3);
        part3StartRotation = zRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, AdjustedAngle(0f, 60f, directions[2]));
    }

    public void setPart4StartRotation(float zRotation, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart4);
        part4StartRotation = zRotation;
        part4.localEulerAngles = new Vector3(270f, 129.6f, AdjustedAngle(0f, 170f, directions[3]));
    }

    // -------------------------
    // Live rotations
    // -------------------------

    public void setPart1Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart1);
        part1.localEulerAngles = new Vector3(180f, 0f, AdjustedAngle(delta + part1StartRotation, 95f, directions[0]));
    }

    public void setPart2Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart2);
        part2.localEulerAngles = new Vector3(270f, 0.185f, AdjustedAngle(delta + part2StartRotation, 290f, directions[1]));
    }

    public void setPart3Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart3);
        part3.localEulerAngles = new Vector3(270f, 129.6f, AdjustedAngle(delta + part3StartRotation, 60f, directions[2]));
    }

    public void setPart4Rotation(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart4);
        part4.localEulerAngles = new Vector3(270f, 129.6f, AdjustedAngle(delta + part4StartRotation, 170f, directions[3]));
    }

    public void setPart1RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart1);
        part1.localEulerAngles = new Vector3(180f, 0f, AdjustedAngle(delta, 95f, directions[0]));
    }

    public void setPart2RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart2);
        part2.localEulerAngles = new Vector3(270f, 0.185f, AdjustedAngle(delta, 290f, directions[1]));
    }

    public void setPart3RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart3);
        part3.localEulerAngles = new Vector3(270f, 129.6f, AdjustedAngle(delta, 60f, directions[2]));
    }

    public void setPart4RotationVisual(float delta, int outlineIndex)
    {
        if (outlineIndex == 1) TriggerOutline(outlinePart4);
        part4.localEulerAngles = new Vector3(270f, 129.6f, AdjustedAngle(delta, 170f, directions[3]));
    }

    // -------------------------
    // Claw logic
    // -------------------------

    public void OpenClaw(int outlineIndex)
    {
        if (outlineIndex == 1)
        {
            TriggerOutline(outlinePart5A);
            TriggerOutline(outlinePart5B);
            TriggerOutline(outlinePart6A);
            TriggerOutline(outlinePart6B);
        }

        part5A.localEulerAngles = new Vector3(270f, 170f, AdjustedAngle(0f, 300f, true));
        part6A.localEulerAngles = new Vector3(90f, 190f, AdjustedAngle(0f, 62f, true));
        part5B.localEulerAngles = new Vector3(270f, 198f, AdjustedAngle(0f, 300f, true));
        part6B.localEulerAngles = new Vector3(270f, 198f, AdjustedAngle(0f, 222f, true));
    }

    public void CloseClaw(int outlineIndex)
    {
        if (outlineIndex == 1)
        {
            TriggerOutline(outlinePart5A);
            TriggerOutline(outlinePart5B);
            TriggerOutline(outlinePart6A);
            TriggerOutline(outlinePart6B);
        }

        part5A.localEulerAngles = new Vector3(270f, 170f, AdjustedAngle(0f, 275f, true));
        part6A.localEulerAngles = new Vector3(90f, 190f, AdjustedAngle(0f, 50f, true));
        part5B.localEulerAngles = new Vector3(270f, 198f, AdjustedAngle(0f, 355f, true));
        part6B.localEulerAngles = new Vector3(270f, 198f, AdjustedAngle(0f, 192f, true));
    }

    public void ApplySavedValues(int[] saveValues)
    {
        setPart1Rotation(saveValues[0], 0);
        setPart2Rotation(saveValues[1], 0);
        setPart3Rotation(saveValues[2], 0);
        setPart4Rotation(saveValues[3], 0);

        if (saveValues.Length > 4 && saveValues[4] == 1)
            CloseClaw(0);
        else
            OpenClaw(0);
    }
}