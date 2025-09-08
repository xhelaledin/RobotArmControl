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
    public float outlineFadeDelay = 0.5f;    // delay before fading (fast)
    public float outlineFadeDuration = 0.4f; // fade animation time (fast)

    // Track a fade coroutine per outline so we can cancel/reset properly
    private readonly Dictionary<Outline, Coroutine> fadeRoutines = new Dictionary<Outline, Coroutine>();

    void Start()
    {
        LoadStartRotationsFromPrefs();
        InitAllOutlines();
    }

    void OnDisable()
    {
        // Clean up running coroutines if the object is disabled
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

        // Cancel any pending fade for THIS outline only
        CancelFade(outline);

        // Show immediately
        outline.enabled = true;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = outlineMaxWidth;

        // Start a new inactivity fade for this outline
        var co = StartCoroutine(FadeOutlineAfterInactivity(outline));
        fadeRoutines[outline] = co;
    }

    private IEnumerator FadeOutlineAfterInactivity(Outline outline)
    {
        // Wait the delay before starting fade
        yield return new WaitForSeconds(outlineFadeDelay);

        // If TriggerOutline was called again during the delay, this coroutine would have been cancelled.
        // Since it wasn't, proceed to fade out.
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

        // Remove handle
        if (outline != null) fadeRoutines.Remove(outline);
    }

    // -------------------------
    // Original rotation methods
    // -------------------------

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

    public void setPart1StartRotation(float zRotation, int outlineIndex)
    {
        if(outlineIndex == 1)
        TriggerOutline(outlinePart1);

        part1StartRotation = zRotation;
        var adjusted = directions[0] ? zRotation : -zRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + adjusted);
    }

    public void setPart2StartRotation(float zRotation, int outlineIndex)
    {
        if(outlineIndex == 1)
        TriggerOutline(outlinePart2);

        part2StartRotation = zRotation;
        var adjusted = directions[1] ? zRotation : -zRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 80 + adjusted);
    }

    public void setPart3StartRotation(float zRotation, int outlineIndex)
    {
        if(outlineIndex == 1)
        TriggerOutline(outlinePart3);

        part3StartRotation = zRotation;
        var adjusted = directions[2] ? zRotation : -zRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 20 + adjusted);
    }

    public void SetDirection(int partIndex, bool isPositive)
    {
        if (partIndex >= 0 && partIndex < directions.Length)
            directions[partIndex] = isPositive;
    }

    public void setPart1Rotation(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
        TriggerOutline(outlinePart1);

        var adj = directions[0] ? rotation : -rotation;
        var angle = adj + part1StartRotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + angle);
    }

    public void setPart2Rotation(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
        TriggerOutline(outlinePart2);

        var adj = directions[1] ? rotation : -rotation;
        var angle = adj + part2StartRotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 80 + angle);
    }

    public void setPart3Rotation(float rotation, int outlineIndex)
    {
        if (outlineIndex == 1)
        TriggerOutline(outlinePart3);

        var adj = directions[2] ? rotation : -rotation;
        var angle = adj + part3StartRotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 20 + angle);
    }

    public void setPart1RotationVisual(float rotation, int outlineIndex)
    {
        if(outlineIndex == 1)
        TriggerOutline(outlinePart1);

        var adj = directions[0] ? rotation : -rotation;
        part1.localEulerAngles = new Vector3(180f, 0f, 90 + adj);
    }

    public void setPart2RotationVisual(float rotation, int outlineIndex)
    {
        if(outlineIndex == 1)
        TriggerOutline(outlinePart2);

        var adj = directions[1] ? rotation : -rotation;
        part2.localEulerAngles = new Vector3(270f, 0.185f, 80 + adj);
    }

    public void setPart3RotationVisual(float rotation, int outlineIndex)
    {
        if(outlineIndex == 1)
        TriggerOutline(outlinePart3);

        var adj = directions[2] ? rotation : -rotation;
        part3.localEulerAngles = new Vector3(270f, 129.6f, 20 + adj);
    }

    public void OpenClaw()
    {
        TriggerOutline(outlinePart4A);
        TriggerOutline(outlinePart4B);
        TriggerOutline(outlinePart5A);
        TriggerOutline(outlinePart5B);

        part4A.localEulerAngles = new Vector3(270f, 170f, 300f);
        part5A.localEulerAngles = new Vector3(90f, 190f, 62f);
        part4B.localEulerAngles = new Vector3(270f, 198f, 300f);
        part5B.localEulerAngles = new Vector3(270f, 198f, 222f);
    }

    public void CloseClaw()
    {
        TriggerOutline(outlinePart4A);
        TriggerOutline(outlinePart4B);
        TriggerOutline(outlinePart5A);
        TriggerOutline(outlinePart5B);

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
            CloseClaw();
        else
            OpenClaw();
    }
}
