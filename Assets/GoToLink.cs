using UnityEngine;
using TMPro;

public class GoToLink : MonoBehaviour, IHideablePanel
{
    public GameObject goToLinkPanel;


    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI linkText;


    private string link;

    public PanelManager panelManager;


    public void ShowGoToLinkPanel(string link, string descriptionText)
    {
        this.link = link;
        linkText.text = link;

        this.descriptionText.text = descriptionText;
        goToLinkPanel.SetActive(true);
        PanelManager.Instance.RegisterPanel(this);
    }

    public void HidePanel()
    {
        goToLinkPanel.SetActive(false);
    }

    public void OnConfirmClicked()
    {
        Application.OpenURL(link);
        HidePanel();
    }

    public bool IsPanelActive() => goToLinkPanel.activeSelf;
}
