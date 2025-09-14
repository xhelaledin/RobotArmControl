using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One toggle item in the AddToList panel. Can be a normal list or "Create New".
/// </summary>
public class AddToListItemManager : MonoBehaviour
{
    public TMP_Text listNameText;
    public Toggle selectToggle;
    public Button createNewButton; // optional button for "Create New"

    public string ListName { get; private set; }
    public bool IsSelected => selectToggle != null && selectToggle.isOn;
    public bool IsCreateNew { get; private set; }

    /// <summary>
    /// Set normal toggle list item.
    /// </summary>
    public void SetData(string listName)
    {
        IsCreateNew = false;
        ListName = listName;
        if (listNameText != null) listNameText.text = listName;
        if (selectToggle != null) selectToggle.isOn = false;
        if (createNewButton != null) createNewButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Configure as "Create New" button item.
    /// </summary>
    public void SetAsCreateNew(System.Action onClickAction)
    {
        IsCreateNew = true;
        if (listNameText != null) listNameText.text = "Create New List";
        if (selectToggle != null) selectToggle.gameObject.SetActive(false);
        if (createNewButton != null)
        {
            createNewButton.gameObject.SetActive(true);
            createNewButton.onClick.RemoveAllListeners();
            createNewButton.onClick.AddListener(() => onClickAction?.Invoke());
        }
    }

    /// <summary>
    /// Programmatically select this toggle.
    /// </summary>
    public void Select()
    {
        if (selectToggle != null) selectToggle.isOn = true;
    }
}
