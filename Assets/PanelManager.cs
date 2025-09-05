using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    private List<IHideablePanel>  registeredPanels  = new List<IHideablePanel>();
    private List<IHideablePanel2> registeredPanels2 = new List<IHideablePanel2>();
    private List<IHideablePanel3> registeredPanels3 = new List<IHideablePanel3>();

    // ===== NEW: unified history of open panels (most recent at the end) =====
    private class PanelHandle
    {
        public GameObject key;          // typically the panel GameObject
        public Action Hide;             // how to hide this panel
        public Func<bool> IsActive;     // how to check if it’s still visible
    }
    private readonly List<PanelHandle> history = new List<PanelHandle>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPanel(IHideablePanel panel)
    {
        if (!registeredPanels.Contains(panel))
            registeredPanels.Add(panel);
    }

    public void RegisterPanel2(IHideablePanel2 panel)
    {
        if (!registeredPanels2.Contains(panel))
            registeredPanels2.Add(panel);
    }

    public void RegisterPanel3(IHideablePanel3 panel)
    {
        if (!registeredPanels3.Contains(panel))
            registeredPanels3.Add(panel);
    }

    // ===== NEW: push/bump a panel onto the unified history stack =====
    public void PushPanel(GameObject key, Action hide, Func<bool> isActive)
    {
        if (key == null || hide == null || isActive == null) return;
        // de-dup/bump: remove any older entries for the same panel key
        history.RemoveAll(h => h.key == key);
        history.Add(new PanelHandle { key = key, Hide = hide, IsActive = isActive });
    }

    public void HideTopActivePanel()
    {
        // Prefer the unified history (true last-opened wins)
        for (int i = history.Count - 1; i >= 0; i--)
        {
            var h = history[i];
            bool active = false;
            try { active = h.IsActive(); } catch { active = false; }

            if (h.key == null)
            {
                history.RemoveAt(i);
                continue;
            }

            if (active)
            {
                try { h.Hide(); } catch { /* swallow */ }
                history.RemoveAt(i);
                return;
            }
            else
            {
                // stale entry, drop it
                history.RemoveAt(i);
            }
        }

        // Fallback (legacy behavior) if history is empty:
        for (int i = registeredPanels.Count - 1; i >= 0; i--)
            if (registeredPanels[i].IsPanelActive()) { registeredPanels[i].HidePanel(); return; }

        for (int i = registeredPanels2.Count - 1; i >= 0; i--)
            if (registeredPanels2[i].IsPanelActive2()) { registeredPanels2[i].HidePanel2(); return; }

        for (int i = registeredPanels3.Count - 1; i >= 0; i--)
            if (registeredPanels3[i].IsPanelActive3()) { registeredPanels3[i].HidePanel3(); return; }
    }

    public bool HasActivePanels()
    {
        foreach (var panel in registeredPanels)
            if (panel.IsPanelActive()) return true;

        foreach (var panel in registeredPanels2)
            if (panel.IsPanelActive2()) return true;

        foreach (var panel in registeredPanels3)
            if (panel.IsPanelActive3()) return true;

        return false;
    }
}
