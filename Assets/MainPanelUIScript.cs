using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainPanelUIScript : MonoBehaviour, IHideablePanel, IHideablePanel2, IHideablePanel3
{
    [Header("Panels")]
    public GameObject controlPanel;
    public GameObject savedPositionsPanel;
    public GameObject listPanel;

    public GameObject controlHeaderPanel;
    public GameObject savedPositionsHeaderPanel;
    public GameObject listHeaderPanel;

    [Header("Images & Sprites")]
    public Image controlImage;
    public Image savedImage;
    public Image listImage;

    public Sprite controlFilled;
    public Sprite controlOutline;
    public Sprite savedFilled;
    public Sprite savedOutline;
    public Sprite listFilled;
    public Sprite listOutline;

    [Header("TMP Texts")]
    public TextMeshProUGUI controlText;
    public TextMeshProUGUI savedText;
    public TextMeshProUGUI listText;

    [Header("Classes")]
    public RobotArmSelection robotArmSelection;
    public ListManager listManager;
    public SaveListManager saveListManager;

    // --- Show Saved Positions Panel ---
    public void ShowSavedPositionsPanel()
    {
        if (savedPositionsPanel != null) savedPositionsPanel.SetActive(true);
        if (controlPanel != null) controlPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(false);

        if (controlImage != null) controlImage.sprite = controlOutline;
        if (savedImage != null) savedImage.sprite = savedFilled;
        if (listImage != null) listImage.sprite = listOutline;

        if (savedText != null) savedText.color = new Color32(0xF4, 0xF4, 0xF4, 0xFF);
        if (controlText != null) controlText.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);
        if (listText != null) listText.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);

        savedPositionsHeaderPanel.SetActive(true);
        controlHeaderPanel.SetActive(false);
        listHeaderPanel.SetActive(false);

        // Push into PanelManager history
        PanelManager.Instance?.PushPanel(
            key: savedPositionsPanel,
            hide: HidePanel,
            isActive: IsPanelActive
        );

        // Keep registration for HasActivePanels()
        PanelManager.Instance.RegisterPanel(this);

        listManager.PopulateList();
    }

    public void HidePanel()
    {
        if (savedPositionsPanel != null) savedPositionsPanel.SetActive(false);
        ShowControlPanel();
        robotArmSelection.MoveModelByStartValues();
    }

    public bool IsPanelActive()
    {
        return savedPositionsPanel != null && savedPositionsPanel.activeSelf;
    }

    // --- Show Control Panel ---
    public void ShowControlPanel()
    {
        if (savedPositionsPanel != null) savedPositionsPanel.SetActive(false);
        if (controlPanel != null) controlPanel.SetActive(true);
        if (listPanel != null) listPanel.SetActive(false);

        if (controlImage != null) controlImage.sprite = controlFilled;
        if (savedImage != null) savedImage.sprite = savedOutline;
        if (listImage != null) listImage.sprite = listOutline;

        if (savedText != null) savedText.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);
        if (controlText != null) controlText.color = new Color32(0xF4, 0xF4, 0xF4, 0xFF);
        if (listText != null) listText.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);

        controlHeaderPanel.SetActive(true);
        savedPositionsHeaderPanel.SetActive(false);
        listHeaderPanel.SetActive(false);

        // Push into PanelManager history
        // PanelManager.Instance?.PushPanel(
        //     key: controlPanel,
        //     hide: HidePanel2,
        //     isActive: IsPanelActive2
        // );

        // PanelManager.Instance.RegisterPanel2(this);
    }

    public void HidePanel2()
    {
        if (controlPanel != null) controlPanel.SetActive(false);
    }

    public bool IsPanelActive2()
    {
        return controlPanel != null && controlPanel.activeSelf;
    }

    // --- Show List Panel ---
    public void ShowListPanel()
    {
        if (savedPositionsPanel != null) savedPositionsPanel.SetActive(false);
        if (controlPanel != null) controlPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(true);

        if (controlImage != null) controlImage.sprite = controlOutline;
        if (savedImage != null) savedImage.sprite = savedOutline;
        if (listImage != null) listImage.sprite = listFilled;

        if (savedText != null) savedText.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);
        if (controlText != null) controlText.color = new Color32(0xAA, 0xAA, 0xAA, 0xFF);
        if (listText != null) listText.color = new Color32(0xF4, 0xF4, 0xF4, 0xFF);

        listHeaderPanel.SetActive(true);
        savedPositionsHeaderPanel.SetActive(false);
        controlHeaderPanel.SetActive(false);

        // Push into PanelManager history
        PanelManager.Instance?.PushPanel(
            key: listPanel,
            hide: HidePanel3,
            isActive: IsPanelActive3
        );

        PanelManager.Instance.RegisterPanel3(this);
    }

    public void HidePanel3()
    {
        if (listPanel != null) listPanel.SetActive(false);
        ShowControlPanel();
        robotArmSelection.MoveModelByStartValues();
        saveListManager.StopActiveCoroutine();
    }

    public bool IsPanelActive3()
    {
        return listPanel != null && listPanel.activeSelf;
    }
}
