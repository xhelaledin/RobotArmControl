using UnityEngine.UI;
using Lean.Gui;

public static class LeanToggleExtensions
{
    public static bool Interactable(this LeanToggle toggle)
    {
        var selectable = toggle.GetComponent<Selectable>();
        return selectable != null && selectable.interactable;
    }

    public static void SetInteractable(this LeanToggle toggle, bool value)
    {
        var selectable = toggle.GetComponent<Selectable>();
        if (selectable != null)
        {
            selectable.interactable = value;
        }
    }
}
