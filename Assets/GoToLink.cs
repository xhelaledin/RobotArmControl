using UnityEngine;
using TMPro;

public class GoToLink : MonoBehaviour
{
    public GameObject goToLinkPanel;

    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI linkText;

    private string link;

    public void ShowGoToLinkPanel(string link, string descriptionText)
    {
        this.link = link;
        
        if (linkText != null)
            linkText.text = link;

        if (this.descriptionText != null)
            this.descriptionText.text = descriptionText;
        
        goToLinkPanel.SetActive(true);

        // --- UPDATED: Replaced RegisterPanel with PushPanel ---
        PanelManager.Instance.PushPanel(
            key: goToLinkPanel,
            hide: HidePanel,      // Pass the existing HidePanel method
            isActive: IsPanelActive  // Pass the existing IsPanelActive method
        );
    }

    // This method is now called by PanelManager's 'hide' delegate
    // or by OnConfirmClicked
    public void HidePanel()
    {
        goToLinkPanel.SetActive(false);
    }

    public void OnConfirmClicked()
    {
        Application.OpenURL(link);
        HidePanel();
    }

    // This method is now called by PanelManager's 'isActive' delegate
    public bool IsPanelActive() => goToLinkPanel.activeSelf;
}