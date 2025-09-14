using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveListItemManager : MonoBehaviour
{
    public TMP_Text listNameText;
    public TMP_Text savesCountText;
    public TMP_Text dateText;

    public Button runButton;
    public Button viewButton;
    public Button deleteButton;

    private string currentListName;
    private System.Action<string> onRun;
    private System.Action<string> onView;
    private System.Action<string> onDelete;
    private ListManager listManager;

    public void SetData(string listName, SaveListData listData,
        System.Action<string> runAction,
        System.Action<string> viewAction,
        System.Action<string> deleteAction,
        ListManager manager)
    {
        currentListName = listName;
        listManager = manager;

        listNameText.text = listName;
        savesCountText.text = $"{listData.saves.Count} saves";
        dateText.text = listData.createdDate;

        onRun = runAction;
        onView = viewAction;
        onDelete = deleteAction;

        runButton.onClick.RemoveAllListeners();
        viewButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        runButton.onClick.AddListener(() => onRun?.Invoke(currentListName));
        viewButton.onClick.AddListener(() => onView?.Invoke(currentListName));
        deleteButton.onClick.AddListener(() => onDelete?.Invoke(currentListName));
    }
}
