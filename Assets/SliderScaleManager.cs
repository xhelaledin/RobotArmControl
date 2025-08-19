using UnityEngine;
using UnityEngine.UI;

public class SliderScaleManager : MonoBehaviour
{
    [Header("Attach the 5 sliders here")]
    public Slider[] sliders = new Slider[5];

    private int selectedModelIndex;

    void Start()
    {
        LoadSelectedModelIndex();
        ApplyScaling();
    }

    /// <summary>
    /// Reloads the selectedModelIndex from PlayerPrefs and applies scaling.
    /// Call this from other scripts when the case changes.
    /// </summary>
    public void ReloadSelectedModel()
    {
        LoadSelectedModelIndex();
        ApplyScaling();
    }

    private void LoadSelectedModelIndex()
    {
        selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);
    }

    private void ApplyScaling()
    {
        switch (selectedModelIndex)
        {
            case 0:
                SetSlidersYScale(11f); // Example scale for case 0
                break;
            case 1:
                SetSlidersYScale(11f); // Example scale for case 1
                break;
            case 2:
                SetSlidersYScale(11f); // Example scale for case 2
                break;
            case 3:
                SetSlidersYScale(9f); // Example scale for case 3
                break;
            default:
                Debug.LogWarning("Unexpected case: " + selectedModelIndex);
                break;
        }
    }

    /// <summary>
    /// Changes the Y scale of all sliders.
    /// </summary>
    private void SetSlidersYScale(float yScale)
    {
        foreach (Slider slider in sliders)
        {
            if (slider != null)
            {
                Vector3 scale = slider.transform.localScale;
                scale.y = yScale;
                slider.transform.localScale = scale;
            }
        }
    }
}
