using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderTextUpdater : MonoBehaviour
{
    [Header("Slider 1")]
    public Slider slider1;
    public TextMeshProUGUI s1min;
    public TextMeshProUGUI s1max;
    public TextMeshProUGUI s1currentvalue;

    [Header("Slider 2")]
    public Slider slider2;
    public TextMeshProUGUI s2min;
    public TextMeshProUGUI s2max;
    public TextMeshProUGUI s2currentvalue;

    [Header("Slider 3")]
    public Slider slider3;
    public TextMeshProUGUI s3min;
    public TextMeshProUGUI s3max;
    public TextMeshProUGUI s3currentvalue;

    [Header("Slider 4")]
    public Slider slider4;
    public TextMeshProUGUI s4min;
    public TextMeshProUGUI s4max;
    public TextMeshProUGUI s4currentvalue;

    [Header("Slider 5")]
    public Slider slider5;
    public TextMeshProUGUI s5min;
    public TextMeshProUGUI s5max;
    public TextMeshProUGUI s5currentvalue;

    [Header("Slider 1 Start")]
    public Slider sliderstart1;
    public TextMeshProUGUI s1startcurrentvalue;

    [Header("Slider 2 Start")]
    public Slider sliderstart2;
    public TextMeshProUGUI s2startcurrentvalue;

    [Header("Slider 3 Start")]
    public Slider sliderstart3;
    public TextMeshProUGUI s3startcurrentvalue;

    [Header("Slider 4 Start")]
    public Slider sliderstart4;
    public TextMeshProUGUI s4startcurrentvalue;

    [Header("Slider 5 Start")]
    public Slider sliderstart5;
    public TextMeshProUGUI s5startcurrentvalue;

    // Internals for iteration
    private Slider[] _sliders;
    private TextMeshProUGUI[] _minTexts;
    private TextMeshProUGUI[] _maxTexts;
    private TextMeshProUGUI[] _currentTexts;
    private bool[] _flipped; // Tracks FlipDirection for each slider

    // Start sliders arrays (only current values matter)
    private Slider[] _startSliders;
    private TextMeshProUGUI[] _startCurrentTexts;

    [Header("Other")]
    public RobotArmSelection robotArmSelection;

    private void Awake()
    {
        _sliders = new[] { slider1, slider2, slider3, slider4, slider5 };
        _minTexts = new[] { s1min, s2min, s3min, s4min, s5min };
        _maxTexts = new[] { s1max, s2max, s3max, s4max, s5max };
        _currentTexts = new[] { s1currentvalue, s2currentvalue, s3currentvalue, s4currentvalue, s5currentvalue };
        _flipped = new bool[5];

        _startSliders = new[] { sliderstart1, sliderstart2, sliderstart3, sliderstart4, sliderstart5 };
        _startCurrentTexts = new[] { s1startcurrentvalue, s2startcurrentvalue, s3startcurrentvalue, s4startcurrentvalue, s5startcurrentvalue };

        // Wire value-changed listeners (for main sliders)
        for (int i = 0; i < _sliders.Length; i++)
        {
            int idx = i; // capture
            if (_sliders[idx] != null)
            {
                _sliders[idx].onValueChanged.AddListener(_ => UpdateCurrentValueText(idx));
            }
        }

        // Wire value-changed listeners (for start sliders → ONLY update their current text)
        for (int i = 0; i < _startSliders.Length; i++)
        {
            int idx = i;
            if (_startSliders[idx] != null)
            {
                _startSliders[idx].onValueChanged.AddListener(_ => UpdateStartCurrentValueText(idx));
            }
        }

        // Initial load from PlayerPrefs
        RefreshAllFromPrefs();
        LoadStartValuesFromPrefs(); // <-- load start sliders from PlayerPrefs
    }

    private void OnDestroy()
    {
        // Clean up listeners
        for (int i = 0; i < _sliders.Length; i++)
        {
            if (_sliders[i] != null)
                _sliders[i].onValueChanged.RemoveAllListeners();
        }

        for (int i = 0; i < _startSliders.Length; i++)
        {
            if (_startSliders[i] != null)
                _startSliders[i].onValueChanged.RemoveAllListeners();
        }
    }

    // Call this after you change PlayerPrefs or when you want to reload everything
    public void RefreshAllFromPrefs()
    {
        for (int i = 0; i < 5; i++)
        {
            ApplyPrefsToSlider(i);
        }
    }

    // Call this if only slider values changed (not PlayerPrefs min/max/flip)
    public void RefreshCurrentTextsOnly()
    {
        for (int i = 0; i < 5; i++)
        {
            UpdateCurrentValueText(i);
        }
    }

    // --- START SLIDER METHODS ---

    /// <summary>
    /// Refresh start sliders’ texts from their current slider values (user-driven).
    /// </summary>
    public void RefreshStartCurrentTextsOnly()
    {
        for (int i = 0; i < 5; i++)
        {
            UpdateStartCurrentValueText(i);
        }
    }

    /// <summary>
    /// Force start slider texts to reload from PlayerPrefs, based on selected model.
    /// </summary>
    public void LoadStartValuesFromPrefs()
    {
        string modelKey = GetModelKeyPrefix();
        int selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0);

        // Decide how many sliders are valid for this model
        int validSliders = 0;
        switch (selectedModelIndex)
        {
            case 0: validSliders = 3; break; // model4 → sliders 1-3
            case 1: validSliders = 4; break; // model5 → sliders 1-4
            case 2: validSliders = 4; break; // model5B → sliders 1-4
            case 3: validSliders = 5; break; // model6 → sliders 1-5
            default: validSliders = 3; break;
        }

        for (int i = 0; i < validSliders; i++)
        {
            var slider = _startSliders[i];
            var curText = _startCurrentTexts[i];
            if (slider == null || curText == null) continue;

            string key = $"{modelKey}startRotationpart{i + 1}";
            float prefValue = PlayerPrefs.GetFloat(key, slider.value);

            // Update ONLY the text to match pref
            curText.text = Mathf.RoundToInt(prefValue).ToString();
        }
    }

    private string GetModelKeyPrefix()
    {
        int selectedModelIndex = PlayerPrefs.GetInt("SelectedModelIndex", 0); // default to model4
        switch (selectedModelIndex)
        {
            case 0: return "model4";
            case 1: return "model5";
            case 2: return "model5B";
            case 3: return "model6";
            default: return "model4";
        }
    }

    // --- MAIN SLIDER METHODS ---

    public void ApplyPrefsToSlider(int index)
    {
        var slider = _sliders[index];
        var minText = _minTexts[index];
        var maxText = _maxTexts[index];

        if (slider == null)
        {
            Debug.LogWarning($"SliderTextUpdater: Slider {index + 1} not assigned in the Inspector.");
            return;
        }

        string baseKey = $"Slider{index + 1}_";

        // Read prefs with safe fallbacks to current slider settings
        int prefMin = PlayerPrefs.GetInt(baseKey + "Min", Mathf.RoundToInt(slider.minValue));
        int prefMax = PlayerPrefs.GetInt(baseKey + "Max", Mathf.RoundToInt(slider.maxValue));
        int start = PlayerPrefs.GetInt(baseKey + "Start", Mathf.RoundToInt(slider.value));
        bool flipped = PlayerPrefs.GetInt(baseKey + "FlipDirection", 0) == 1;

        // Normalize range
        int low = Mathf.Min(prefMin, prefMax);
        int high = Mathf.Max(prefMin, prefMax);

        // Configure slider as whole numbers (since prefs are ints)
        slider.wholeNumbers = true;
        slider.minValue = low;
        slider.maxValue = high;
        slider.value = Mathf.Clamp(start, low, high);

        // Store flip state for this index
        _flipped[index] = flipped;

        // Update endpoint labels (swap when flipped)
        if (minText != null) minText.text = (flipped ? high : low).ToString();
        if (maxText != null) maxText.text = (flipped ? low : high).ToString();

        // Update current value text
        UpdateCurrentValueText(index);

        robotArmSelection.MoveModelByStartValues();
    }

    private void UpdateCurrentValueText(int index)
    {
        var slider = _sliders[index];
        var curText = _currentTexts[index];
        if (slider == null || curText == null) return;

        int low = Mathf.RoundToInt(slider.minValue);
        int high = Mathf.RoundToInt(slider.maxValue);
        int raw = Mathf.RoundToInt(slider.value);

        // If flipped, display the mirrored value so the number matches the swapped endpoints
        int display = _flipped[index] ? (low + high - raw) : raw;
        curText.text = display.ToString();
    }

    public void UpdateStartCurrentValueText(int index)
    {
        var slider = _startSliders[index];
        var curText = _startCurrentTexts[index];
        if (slider == null || curText == null) return;

        int raw = Mathf.RoundToInt(slider.value);
        curText.text = raw.ToString();
    }
}
