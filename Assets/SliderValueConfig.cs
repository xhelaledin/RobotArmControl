using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class SliderValueConfig : MonoBehaviour
{
    public RobotArmSelection robotArmSelection;

    public GameObject sliderConfigPanel;
    public TMP_InputField slider1MinValueIn, slider1MaxValueIn, slider1StartValueIn;
    public TMP_InputField slider2MinValueIn, slider2MaxValueIn, slider2StartValueIn;
    public TMP_InputField slider3MinValueIn, slider3MaxValueIn, slider3StartValueIn;
    public TMP_InputField slider4MinValueIn, slider4MaxValueIn, slider4StartValueIn;
    public TMP_InputField slider5MinValueIn, slider5MaxValueIn, slider5StartValueIn;
    public Toggle slider1DirectionToggle, slider2DirectionToggle, slider3DirectionToggle, slider4DirectionToggle, slider5DirectionToggle;
    public TMP_InputField openButtonValueIn;
    public TMP_InputField closeButtonValueIn;

    void Start()
    {
        LoadSliderValues();
    }

    private int ParseIntOrDefault(TMP_InputField field, int defaultValue)
    {
        if (!int.TryParse(field.text, out int result))
        {
            return defaultValue;
        }
        return result;
    }

    public void OnSliderConfirmClicked()
    {
        OnSlider1ConfirmClicked();
        OnSlider2ConfirmClicked();
        OnSlider3ConfirmClicked();
        OnSlider4ConfirmClicked();
        OnSlider5ConfirmClicked();
        OnButtonConfirmClicked();
    }

    public void OnSlider1ConfirmClicked()
    {
        int min = ParseIntOrDefault(slider1MinValueIn, 0);
        int max = ParseIntOrDefault(slider1MaxValueIn, 180);
        int start = ParseIntOrDefault(slider1StartValueIn, 90);

        min = Mathf.Max(min, 0);
        max = Mathf.Max(max, min);
        start = Mathf.Clamp(start, min, max);

        bool flipDirection = slider1DirectionToggle.isOn;

        robotArmSelection.ConfigureSliderValue(0, min, max, start, flipDirection);

        SaveSliderValues(1, min, max, start, flipDirection);
    }

    public void OnSlider2ConfirmClicked()
    {
        int min = ParseIntOrDefault(slider2MinValueIn, 0);
        int max = ParseIntOrDefault(slider2MaxValueIn, 180);
        int start = ParseIntOrDefault(slider2StartValueIn, 90);

        min = Mathf.Max(min, 0);
        max = Mathf.Max(max, min);
        start = Mathf.Clamp(start, min, max);

        bool flipDirection = slider2DirectionToggle.isOn;

        robotArmSelection.ConfigureSliderValue(1, min, max, start, flipDirection);

        SaveSliderValues(2, min, max, start, flipDirection);
    }

    public void OnSlider3ConfirmClicked()
    {
        int min = ParseIntOrDefault(slider3MinValueIn, 0);
        int max = ParseIntOrDefault(slider3MaxValueIn, 180);
        int start = ParseIntOrDefault(slider3StartValueIn, 90);

        min = Mathf.Max(min, 0);
        max = Mathf.Max(max, min);
        start = Mathf.Clamp(start, min, max);

        bool flipDirection = slider3DirectionToggle.isOn;

        robotArmSelection.ConfigureSliderValue(2, min, max, start, flipDirection);

        SaveSliderValues(3, min, max, start, flipDirection);
    }

    public void OnSlider4ConfirmClicked()
    {
        int min = ParseIntOrDefault(slider4MinValueIn, 0);
        int max = ParseIntOrDefault(slider4MaxValueIn, 180);
        int start = ParseIntOrDefault(slider4StartValueIn, 90);

        min = Mathf.Max(min, 0);
        max = Mathf.Max(max, min);
        start = Mathf.Clamp(start, min, max);

        bool flipDirection = slider4DirectionToggle.isOn;

        robotArmSelection.ConfigureSliderValue(3, min, max, start, flipDirection);

        SaveSliderValues(4, min, max, start, flipDirection);
    }

    public void OnSlider5ConfirmClicked()
    {
        int min = ParseIntOrDefault(slider5MinValueIn, 0);
        int max = ParseIntOrDefault(slider5MaxValueIn, 180);
        int start = ParseIntOrDefault(slider5StartValueIn, 90);

        min = Mathf.Max(min, 0);
        max = Mathf.Max(max, min);
        start = Mathf.Clamp(start, min, max);

        bool flipDirection = slider5DirectionToggle.isOn;

        robotArmSelection.ConfigureSliderValue(4, min, max, start, flipDirection);

        SaveSliderValues(5, min, max, start, flipDirection);
    }

    public void OnButtonConfirmClicked()
    {
        int open = ParseIntOrDefault(openButtonValueIn, 105);
        int close = ParseIntOrDefault(closeButtonValueIn, 177);

        open = Mathf.Max(open, 0);
        close = Mathf.Max(close, 0);

        robotArmSelection.ConfigureOpenCloseValues(open, close);

        SaveButtonValues(open, close);
    }

    private void SaveSliderValues(int index, int min, int max, int start, bool flipDirection)
    {
        PlayerPrefs.SetInt($"Slider{index}_Min", min);
        PlayerPrefs.SetInt($"Slider{index}_Max", max);
        PlayerPrefs.SetInt($"Slider{index}_Start", start);
        PlayerPrefs.SetInt($"Slider{index}_FlipDirection", flipDirection ? 1 : 0); // Save as 1 or 0
        PlayerPrefs.Save();
    }


    private void SaveButtonValues(int open, int close)
    {
        PlayerPrefs.SetInt("OpenButtonValue", open);
        PlayerPrefs.SetInt("CloseButtonValue", close);
        PlayerPrefs.Save();
    }

    public void LoadSliderValues()
    {
        for (int i = 1; i <= 5; i++)
        {
            int min = PlayerPrefs.GetInt($"Slider{i}_Min", 0);
            int max = PlayerPrefs.GetInt($"Slider{i}_Max", 180);
            int start = PlayerPrefs.GetInt($"Slider{i}_Start", 90);
            bool flipDirection = PlayerPrefs.GetInt($"Slider{i}_FlipDirection", 0) == 1;

            robotArmSelection.ConfigureSliderValue(i - 1, min, max, start, flipDirection);

            switch (i)
            {
                case 1:
                    // slider1MinValueIn.placeholder.GetComponent<TMP_Text>().text = min.ToString();
                    // slider1MaxValueIn.placeholder.GetComponent<TMP_Text>().text = max.ToString();
                    // slider1StartValueIn.placeholder.GetComponent<TMP_Text>().text = start.ToString();
                    // slider1DirectionToggle.isOn = flipDirection;

                    slider1MinValueIn.text = min.ToString();
                    slider1MaxValueIn.text = max.ToString();
                    slider1StartValueIn.text = start.ToString();
                    slider1DirectionToggle.isOn = flipDirection;

                    break;
                case 2:
                    // slider2MinValueIn.placeholder.GetComponent<TMP_Text>().text = min.ToString();
                    // slider2MaxValueIn.placeholder.GetComponent<TMP_Text>().text = max.ToString();
                    // slider2StartValueIn.placeholder.GetComponent<TMP_Text>().text = start.ToString();
                    // slider2DirectionToggle.isOn = flipDirection;

                    slider2MinValueIn.text = min.ToString();
                    slider2MaxValueIn.text = max.ToString();
                    slider2StartValueIn.text = start.ToString();
                    slider2DirectionToggle.isOn = flipDirection;
                    break;
                case 3:
                    // slider3MinValueIn.placeholder.GetComponent<TMP_Text>().text = min.ToString();
                    // slider3MaxValueIn.placeholder.GetComponent<TMP_Text>().text = max.ToString();
                    // slider3StartValueIn.placeholder.GetComponent<TMP_Text>().text = start.ToString();
                    // slider3DirectionToggle.isOn = flipDirection;

                    slider3MinValueIn.text = min.ToString();
                    slider3MaxValueIn.text = max.ToString();
                    slider3StartValueIn.text = start.ToString();
                    slider3DirectionToggle.isOn = flipDirection;
                    break;
                case 4:
                    // slider4MinValueIn.placeholder.GetComponent<TMP_Text>().text = min.ToString();
                    // slider4MaxValueIn.placeholder.GetComponent<TMP_Text>().text = max.ToString();
                    // slider4StartValueIn.placeholder.GetComponent<TMP_Text>().text = start.ToString();
                    // slider4DirectionToggle.isOn = flipDirection;

                    slider4MinValueIn.text = min.ToString();
                    slider4MaxValueIn.text = max.ToString();
                    slider4StartValueIn.text = start.ToString();
                    slider4DirectionToggle.isOn = flipDirection;
                    break;
                case 5:
                    // slider5MinValueIn.placeholder.GetComponent<TMP_Text>().text = min.ToString();
                    // slider5MaxValueIn.placeholder.GetComponent<TMP_Text>().text = max.ToString();
                    // slider5StartValueIn.placeholder.GetComponent<TMP_Text>().text = start.ToString();
                    // slider5DirectionToggle.isOn = flipDirection;

                    slider5MinValueIn.text = min.ToString();
                    slider5MaxValueIn.text = max.ToString();
                    slider5StartValueIn.text = start.ToString();
                    slider5DirectionToggle.isOn = flipDirection;
                    break;
            }
        }

        int open = PlayerPrefs.GetInt("OpenButtonValue", 105);
        int close = PlayerPrefs.GetInt("CloseButtonValue", 177);

        robotArmSelection.ConfigureOpenCloseValues(open, close);

        // openButtonValueIn.placeholder.GetComponent<TMP_Text>().text = open.ToString();
        // closeButtonValueIn.placeholder.GetComponent<TMP_Text>().text = close.ToString();

        openButtonValueIn.text = open.ToString();
        closeButtonValueIn.text = close.ToString();
    }

    public void ShowSliderConfigPanel()
    {
        sliderConfigPanel.SetActive(true);
    }

    public void HideSliderConfigPanel()
    {
        sliderConfigPanel.SetActive(false);
    }

    public void ConfirmClickTest()
    {
        OnSliderConfirmClicked();
        HideSliderConfigPanel();
    }
}
