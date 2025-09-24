using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveListEntryManager : MonoBehaviour
{
    public TMP_Text positionText;
    public TMP_Text dateText;
    public TMP_Text valuesText;
    public TMP_InputField delayInput;

    public Button viewButton;
    public Button moveUpButton;
    public Button moveDownButton;
    public Button removeButton;

    private SaveReference saveRef;
    private int index;
    private SaveListItemManager parentList;
    private SaveListManager saveListManager;
    private ListManager listManager;

    public void Setup(SaveReference reference, int idx, SaveListItemManager parent, SaveListManager saveMgr, ListManager listMgr)
    {
        saveRef = reference;
        index = idx;
        parentList = parent;
        saveListManager = saveMgr;
        listManager = listMgr;

        positionText.text = reference.saveName;
        dateText.text = reference.dateString;
        valuesText.text = string.Join(", ", reference.values);
        delayInput.text = reference.delayMs.ToString();

        viewButton.onClick.RemoveAllListeners();
        moveUpButton.onClick.RemoveAllListeners();
        moveDownButton.onClick.RemoveAllListeners();
        removeButton.onClick.RemoveAllListeners();
        delayInput.onEndEdit.RemoveAllListeners();

        viewButton.onClick.AddListener(() => parentList.ViewEntry(index));
        moveUpButton.onClick.AddListener(() => parentList.MoveEntry(index, -1));
        moveDownButton.onClick.AddListener(() => parentList.MoveEntry(index, 1));
        removeButton.onClick.AddListener(() => parentList.RemoveEntry(index));

        delayInput.onEndEdit.AddListener(OnDelayChanged);
    }

    private void OnDelayChanged(string newValue)
    {
        if (int.TryParse(newValue, out int newDelay))
            parentList.UpdateDelay(index, newDelay);
        else
            delayInput.text = saveRef.delayMs.ToString();
    }
}
