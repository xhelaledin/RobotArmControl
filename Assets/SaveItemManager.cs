using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SaveItemManager : MonoBehaviour
{
    public TMP_Text positionTitle;
    public TMP_Text valuesText;
    public TMP_Text dateText;

    public Button deleteButton;
    public Button runButton;
    public Button saveButton;   // This is actually "view" button
    public Button addToListButton; // ✅ New button

    public Sprite runNormalSprite;
    public Sprite runSelectedSprite;
    public Sprite viewNormalSprite;
    public Sprite viewSelectedSprite;

    // Set UI text data
    public void SetData(string saveName, int[] values, string date)
    {
        positionTitle.text = saveName;

        if (values.Length > 0)
        {
            if (values[values.Length - 1] == 0)
                valuesText.text = string.Join(", ", values.Take(values.Length - 1)) + ", Open";
            else
                valuesText.text = string.Join(", ", values.Take(values.Length - 1)) + ", Closed";
        }
        else
        {
            valuesText.text = "No values";
        }

        dateText.text = date;

        // Initialize buttons to normal sprites
        SetRunButtonNormal();
        SetViewButtonNormal();
    }

    public void SetupButtons(
        string saveName,
        System.Action<string, GameObject> deleteAction,
        System.Action<string, Button> runAction,
        System.Action<string, Button> viewAction,
        System.Action<string> addToListAction  // ✅ New action
    )
    {
        deleteButton.onClick.AddListener(() => deleteAction.Invoke(saveName, this.gameObject));
        runButton.onClick.AddListener(() => runAction.Invoke(saveName, runButton));
        saveButton.onClick.AddListener(() => viewAction.Invoke(saveName, saveButton));
        addToListButton.onClick.AddListener(() => addToListAction.Invoke(saveName)); // ✅
    }

    // Methods to update button sprites externally
    public void SetRunButtonNormal()
    {
        runButton.GetComponent<Image>().sprite = runNormalSprite;
    }

    public void SetRunButtonSelected()
    {
        runButton.GetComponent<Image>().sprite = runSelectedSprite;
    }

    public void SetViewButtonNormal()
    {
        saveButton.GetComponent<Image>().sprite = viewNormalSprite;
    }

    public void SetViewButtonSelected()
    {
        saveButton.GetComponent<Image>().sprite = viewSelectedSprite;
    }
}
