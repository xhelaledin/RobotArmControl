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
    public TextMeshProUGUI s1name;

    [Header("Slider 2")]
    public Slider slider2;
    public TextMeshProUGUI s2min;
    public TextMeshProUGUI s2max;
    public TextMeshProUGUI s2currentvalue;
    public TextMeshProUGUI s2name;

    [Header("Slider 3")]
    public Slider slider3;
    public TextMeshProUGUI s3min;
    public TextMeshProUGUI s3max;
    public TextMeshProUGUI s3currentvalue;
    public TextMeshProUGUI s3name;

    [Header("Slider 4")]
    public Slider slider4;
    public TextMeshProUGUI s4min;
    public TextMeshProUGUI s4max;
    public TextMeshProUGUI s4currentvalue;
    public TextMeshProUGUI s4name;

    [Header("Slider 5")]
    public Slider slider5;
    public TextMeshProUGUI s5min;
    public TextMeshProUGUI s5max;
    public TextMeshProUGUI s5currentvalue;
    public TextMeshProUGUI s5name;

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
    private TextMeshProUGUI[] _nameTexts;
    private Vector2[] _defaultNameAnchoredPositions;
    private Vector2[] _defaultMinAnchoredPositions;
    private Vector2[] _defaultMaxAnchoredPositions;
    private bool[] _flipped; // Tracks FlipDirection for each slider

    // Start sliders arrays (only current values matter)
    private Slider[] _startSliders;
    private TextMeshProUGUI[] _startCurrentTexts;

    [Header("Other")]
    public RobotArmSelection robotArmSelection;

    // Target anchored position when flipped (converted from your Vector3)
    private readonly Vector2 flippedNameAnchoredPos = new Vector2(-58.81668f, -6.267036f);

    private void Awake()
    {
        // build arrays
        _sliders = new[] { slider1, slider2, slider3, slider4, slider5 };
        _minTexts = new[] { s1min, s2min, s3min, s4min, s5min };
        _maxTexts = new[] { s1max, s2max, s3max, s4max, s5max };
        _currentTexts = new[] { s1currentvalue, s2currentvalue, s3currentvalue, s4currentvalue, s5currentvalue };
        _nameTexts = new[] { s1name, s2name, s3name, s4name, s5name };
        _flipped = new bool[5];

        _startSliders = new[] { sliderstart1, sliderstart2, sliderstart3, sliderstart4, sliderstart5 };
        _startCurrentTexts = new[] { s1startcurrentvalue, s2startcurrentvalue, s3startcurrentvalue, s4startcurrentvalue, s5startcurrentvalue };

        // Save default anchored positions for min/max/name (use anchoredPosition so it works properly with UI)
        _defaultNameAnchoredPositions = new Vector2[_nameTexts.Length];
        _defaultMinAnchoredPositions = new Vector2[_minTexts.Length];
        _defaultMaxAnchoredPositions = new Vector2[_maxTexts.Length];

        for (int i = 0; i < _nameTexts.Length; i++)
        {
            if (_nameTexts[i] != null)
                _defaultNameAnchoredPositions[i] = _nameTexts[i].rectTransform.anchoredPosition;
            else
                _defaultNameAnchoredPositions[i] = Vector2.zero;

            if (_minTexts[i] != null)
                _defaultMinAnchoredPositions[i] = _minTexts[i].rectTransform.anchoredPosition;
            else
                _defaultMinAnchoredPositions[i] = Vector2.zero;

            if (_maxTexts[i] != null)
                _defaultMaxAnchoredPositions[i] = _maxTexts[i].rectTransform.anchoredPosition;
            else
                _defaultMaxAnchoredPositions[i] = Vector2.zero;
        }

        // Apply stored preferences first (ensures UI state is correct on start)
        RefreshAllFromPrefs();
        LoadStartValuesFromPrefs();

        // Wire value-changed listeners AFTER applying prefs (avoids race conditions on startup)
        for (int i = 0; i < _sliders.Length; i++)
        {
            int idx = i;
            if (_sliders[idx] != null)
            {
                // Use SetValueWithoutNotify if you later set value programmatically and want to avoid recursion;
                // here we just ensure changes by the user update the UI text.
                _sliders[idx].onValueChanged.AddListener(_ => UpdateCurrentValueText(idx));
            }
        }

        for (int i = 0; i < _startSliders.Length; i++)
        {
            int idx = i;
            if (_startSliders[idx] != null)
            {
                _startSliders[idx].onValueChanged.AddListener(_ => UpdateStartCurrentValueText(idx));
            }
        }
    }

    private void OnDestroy()
    {
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
        if (index < 0 || index >= _sliders.Length) return;

        var slider = _sliders[index];
        var minText = _minTexts[index];
        var maxText = _maxTexts[index];
        var nameText = _nameTexts[index];

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

        // Clamp and apply start value
        slider.value = Mathf.Clamp(start, low, high);

        // Remember flip state
        _flipped[index] = flipped;

        // Set slider direction (visual only)
        slider.direction = flipped ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;

        // ALWAYS set the numeric min/max text to the actual low/high values.
        // We swap label positions so the visual left/right will show the correct numbers.
        if (minText != null) minText.text = low.ToString();
        if (maxText != null) maxText.text = high.ToString();

        // Swap min/max anchored positions if flipped, otherwise restore defaults
        if (minText != null && maxText != null)
        {
            if (flipped)
            {
                minText.rectTransform.anchoredPosition = _defaultMaxAnchoredPositions[index];
                maxText.rectTransform.anchoredPosition = _defaultMinAnchoredPositions[index];
            }
            else
            {
                minText.rectTransform.anchoredPosition = _defaultMinAnchoredPositions[index];
                maxText.rectTransform.anchoredPosition = _defaultMaxAnchoredPositions[index];
            }
        }

        // Update name text (moves to given anchored pos when flipped)
        if (nameText != null)
        {
            nameText.rectTransform.anchoredPosition = flipped ? flippedNameAnchoredPos : _defaultNameAnchoredPositions[index];
        }

        // Show the actual slider.value in the current-value text (matches Inspector)
        UpdateCurrentValueText(index);

        // Keep rest of system informed
        robotArmSelection?.MoveModelByStartValues();
    }

    private void UpdateCurrentValueText(int index)
    {
        if (index < 0 || index >= _sliders.Length) return;

        var slider = _sliders[index];
        var curText = _currentTexts[index];
        if (slider == null || curText == null) return;

        // Show the slider's numeric value (rounded) — matches the Inspector's value
        int raw = Mathf.RoundToInt(slider.value);
        curText.text = raw.ToString();
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
