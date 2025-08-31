using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;
using System.Collections.Generic;

public class ModelSelectorRadio : MonoBehaviour
{
    [Header("Core References")]
    public RobotArmSelection robotArmSelection;
    public SaveManager       saveManager;
    public ListManager       listManager;
    public GameObject        settingsPanel;

    [Header("Radio Toggles (0 → 3)")]
    public List<LeanToggle>  modelToggles;

    [Header("Preview Image & Sprites")]
    public Image             previewImage;
    public List<Sprite>      modelSprites;

    private const string     MODEL_PREF_KEY = "SelectedModelIndex";
    private int              currentIndex;
    private bool             isProgrammatic;  // true when we’re toggling siblings in code

    void Start()
    {
        // Restore last-saved index
        currentIndex = PlayerPrefs.GetInt(MODEL_PREF_KEY, 0);

        // Hook each toggle
        for (int i = 0; i < modelToggles.Count; i++)
        {
            int idx = i; // capture for the closures

            // We’ll manage sibling-off logic ourselves
            modelToggles[idx].TurnOffSiblings = false;

            // When toggled ON, swap model
            modelToggles[idx].OnOn.AddListener(() => SelectModel(idx));

            // When toggled OFF, if it was the active one and it wasn’t our own code,
            // snap it back on instantly by setting On = true
            modelToggles[idx].OnOff.AddListener(() =>
            {
                if (idx == currentIndex && !isProgrammatic)
                {
                    modelToggles[idx].On = true;
                }
            });
        }

        // Initialize UI (no transitions)
        SelectModel(currentIndex, playTransitions: false);
    }

    private void SelectModel(int newIndex, bool playTransitions = true)
    {
        // Ignore clicking the already-active toggle
        if (newIndex == currentIndex && playTransitions)
            return;

        // 1. Turn ON the new toggle (with or without animation)
        if (playTransitions)
            modelToggles[newIndex].TurnOn();
        else
            modelToggles[newIndex].On = true;

        // 2. Turn OFF siblings under our control
        isProgrammatic = true;
        for (int i = 0; i < modelToggles.Count; i++)
        {
            if (i == newIndex) continue;
            modelToggles[i].TurnOff();
        }
        isProgrammatic = false;

        // 3. Persist selection
        currentIndex = newIndex;
        PlayerPrefs.SetInt(MODEL_PREF_KEY, currentIndex);
        PlayerPrefs.Save();

        // 4. Notify other systems
        robotArmSelection?.OnModelSelected(currentIndex);
        robotArmSelection?.UpdateSelectedModelIndex();
        saveManager?.UpdateSelectedModelIndex(currentIndex);
        listManager?.PopulateList();

        // 5. Swap preview sprite
        if (previewImage != null && currentIndex < modelSprites.Count)
            previewImage.sprite = modelSprites[currentIndex];

        robotArmSelection.MoveModelByStartValues();
    }

    // Optional panel show/hide
    public void ShowSettingsPanel() => settingsPanel.SetActive(true);
    public void HideSettingsPanel() => settingsPanel.SetActive(false);
}
