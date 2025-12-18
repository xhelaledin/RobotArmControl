using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotArmInputHandler4Parts : MonoBehaviour
{
    public Transform part1, part2, part3;
    public Transform part4A, part4B, part5A, part5B;

    [Header("Outline References")]
    public Outline outlinePart1;
    public Outline outlinePart2;
    public Outline outlinePart3;
    public Outline outlinePart4A, outlinePart4B, outlinePart5A, outlinePart5B;

    private bool[] directions = new bool[5];
    private float part1StartRotation;
    private float part2StartRotation;
    private float part3StartRotation;

    [Header("Outline Settings")]
    public float outlineMaxWidth = 5f;
    public float outlineFadeDelay = 0.5f;
    public float outlineFadeDuration = 0.4f;

    private readonly Dictionary<Outline, Coroutine> fadeRoutines = new Dictionary<Outline, Coroutine>();

    void Start()
    {
        LoadStartRotationsFromPrefs();
        InitAllOutlines();
    }

    void OnDisable()
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
        DisableOutline(outlinePart4A);
        DisableOutline(outlinePart4B);
        DisableOutline(outlinePart5A);
        DisableOutline(outlinePart5B);
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

    public void LoadStartRotationsFromPrefs()
    {
        // USING REGISTRY FOR DEFAULTS
        part1StartRotation = PlayerPrefsKeyRegistry.GetFloat("model4startRotationpart1");
        part2StartRotation = PlayerPrefsKeyRegistry.GetFloat("model4startRotationpart2");
        part3StartRotation = PlayerPrefsKeyRegistry.GetFloat("model4startRotationpart3");

        ApplyAllStartRotations();
    }

    private void ApplyAllStartRotations()
    {
        setPart1RotationVisual(0f, 0);
        setPart2RotationVisual(0f, 0);
        setPart3RotationVisual(0f, 0);
    }

    public void SetDirection(int partIndex, bool isPositive)
    {
        if (partIndex >= 0 && partIndex < directions.Length)
            directions[partIndex] = isPositive;
    }

    private float AdjustedAngle(float rotation, float offset, bool reversed)
    {
        float angle;
        if (!reversed)
        {
            angle = offset + rotation;
        }
        else
        {
            angle = offset + (360 - rotation);
        }

        // Keep angle in [0,360)
        angle = angle % 360f;
        return angle;
    }

    public void setPart1Rotation(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
            TriggerOutline(outlinePart1);

        float angle = AdjustedAngle(rotation + part1StartRotation, 90f, directions[0]);
        part1.localEulerAngles = new Vector3(180f, 0f, angle);
    }

    public void setPart2Rotation(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
            TriggerOutline(outlinePart2);

        float angle = AdjustedAngle(rotation + part2StartRotation, 170f, directions[1]);
        part2.localEulerAngles = new Vector3(270f, 0.185f, angle);
    }

    public void setPart3Rotation(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
            TriggerOutline(outlinePart3);

        float angle = AdjustedAngle(rotation + part3StartRotation, 110f, directions[2]);
        part3.localEulerAngles = new Vector3(270f, 129.6f, angle);
    }

    public void setPart1RotationVisual(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
            TriggerOutline(outlinePart1);

        float angle = AdjustedAngle(rotation, 90f, directions[0]);
        part1.localEulerAngles = new Vector3(180f, 0f, angle);
    }

    public void setPart2RotationVisual(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
            TriggerOutline(outlinePart2);

        float angle = AdjustedAngle(rotation, 170f, directions[1]);
        part2.localEulerAngles = new Vector3(270f, 0.185f, angle);
    }

    public void setPart3RotationVisual(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
            TriggerOutline(outlinePart3);

        float angle = AdjustedAngle(rotation, 110f, directions[2]);
        part3.localEulerAngles = new Vector3(270f, 129.6f, angle);
    }

    public void OpenClaw(int outlineIndex)
    {
        if (outlineIndex == 1)
        {
            TriggerOutline(outlinePart4A);
            TriggerOutline(outlinePart4B);
            TriggerOutline(outlinePart5A);
            TriggerOutline(outlinePart5B);
        }

        part4A.localEulerAngles = new Vector3(270f, 170f, 300f);
        part5A.localEulerAngles = new Vector3(90f, 190f, 62f);
        part4B.localEulerAngles = new Vector3(270f, 198f, 300f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 222f);
    }

    public void CloseClaw(int outlineIndex)
    {
        if (outlineIndex == 1)
        {
            TriggerOutline(outlinePart4A);
            TriggerOutline(outlinePart4B);
            TriggerOutline(outlinePart5A);
            TriggerOutline(outlinePart5B);
        }

        part4A.localEulerAngles = new Vector3(270f, 170f, 275f);
        part5A.localEulerAngles = new Vector3(90f, 190f, 50f);
        part4B.localEulerAngles = new Vector3(270f, 198f, 355f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 192f);
    }

    public void ApplySavedValues(int[] saveValues)
    {
        setPart1Rotation(saveValues[0], 0);
        setPart2Rotation(saveValues[1], 0);
        setPart3Rotation(saveValues[2], 0);

        if (saveValues.Length > 3 && saveValues[3] == 1)
            CloseClaw(0);
        else
            OpenClaw(0);
    }
}