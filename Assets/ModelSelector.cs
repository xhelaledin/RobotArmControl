using UnityEngine;
using TMPro;

public class ModelSelector : MonoBehaviour
{

    public RobotArmSelection robotArmSelection;

    public TMP_Dropdown modelDropdown;

    private const string MODEL_PREF_KEY = "SelectedModelIndex";

    public GameObject settingsPanel;

    public SaveManager saveManager;
    public ListManager listManager;

    void Start()
    {
        int savedIndex = PlayerPrefs.GetInt(MODEL_PREF_KEY, 0);
        modelDropdown.value = savedIndex;
        modelDropdown.RefreshShownValue();

        modelDropdown.onValueChanged.AddListener(OnModelDropdownChanged);
    }

    public void OnModelDropdownChanged(int newIndex)
    {
        PlayerPrefs.SetInt(MODEL_PREF_KEY, newIndex);
        PlayerPrefs.Save();

        robotArmSelection.OnModelSelected(newIndex);
        robotArmSelection.UpdateSelectedModelIndex();

        if (saveManager != null)
        {
            saveManager.UpdateSelectedModelIndex(newIndex);
        }

        if (listManager != null)
        {
            listManager.PopulateList();
        }
    }


    public void ShowSettingsPanel()
    {
        settingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }
}
