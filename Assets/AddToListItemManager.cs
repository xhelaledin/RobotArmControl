using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddToListItemManager : MonoBehaviour
{
    public TMP_Text listNameText;
    public Button selectButton;

    private string currentListName;
    private System.Action onSelect;

    public void SetData(string listName, System.Action selectAction)
    {
        currentListName = listName;
        listNameText.text = listName;
        onSelect = selectAction;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect?.Invoke());
    }
}
