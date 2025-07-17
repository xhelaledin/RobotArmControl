using UnityEngine;

public class ShowSavedPositions : MonoBehaviour
{
    public GameObject savedPositionsPanel;
    public GameObject controlPanel;

    public void ShowSavedPositionsPanel()
    {
        if (savedPositionsPanel != null)
        {
            savedPositionsPanel.SetActive(true);
        }

    }

    public void HideSavedPositionsPanel()
    {
        if (savedPositionsPanel != null)
        {
            savedPositionsPanel.SetActive(false);
        }
    }

    public void ShowControlPanel()
    {
        if (controlPanel != null)
        {
            controlPanel.SetActive(true);
        }

    }

    public void HideControlPanel()
    {
        if (controlPanel != null)
        {
            controlPanel.SetActive(false);
        }
    }
}
