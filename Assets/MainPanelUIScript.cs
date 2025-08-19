using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainPanelUIScript : MonoBehaviour
{   
    [Header("Panels")]
    public GameObject savedPositionsPanel;
    public GameObject controlPanel;

    public GameObject savedPositionsHeaderPanel;
    public GameObject controlHeaderPanel;

    [Header("Images & Sprites")]
    public Image image1;
    public Image image2;
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;
    public Sprite sprite4;

    [Header("TMP Texts")]
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;

    // Show/Hide Saved Positions Panel
    public void ShowSavedPositionsPanel()
    {
        if (savedPositionsPanel != null)
            savedPositionsPanel.SetActive(true);
    }

    public void HideSavedPositionsPanel()
    {
        if (savedPositionsPanel != null)
            savedPositionsPanel.SetActive(false);
    }

    // Show/Hide Control Panel with sprite & color swaps
    public void ShowControlPanel()
    {
        if (controlPanel != null)
        {
            controlPanel.SetActive(true);

            if (image1 != null && sprite1 != null)
                image1.sprite = sprite1;

            if (image2 != null && sprite4 != null)
                image2.sprite = sprite4;

            if (text1 != null)
                text1.color = new Color32(0xF4, 0xF4, 0xF4, 0xFF);   // #f4f4f4

            if (text2 != null)
                text2.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);   // #aaaaaa
        }

        controlHeaderPanel.SetActive(true);
        savedPositionsHeaderPanel.SetActive(false);
    }

    public void HideControlPanel()
    {
        if (controlPanel != null)
        {
            controlPanel.SetActive(false);

            if (image1 != null && sprite2 != null)
                image1.sprite = sprite2;

            if (image2 != null && sprite3 != null)
                image2.sprite = sprite3;

            if (text1 != null)
                text1.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);   // #aaaaaa

            if (text2 != null)
                text2.color = new Color32(0xF4, 0xF4, 0xF4, 0xFF);   // #f4f4f4
        }

        controlHeaderPanel.SetActive(false);
        savedPositionsHeaderPanel.SetActive(true);
    }
}