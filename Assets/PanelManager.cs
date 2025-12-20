using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    // ===== Unified history of open panels (most recent at the end) =====
    private class PanelHandle
    {
        public GameObject key;      // typically the panel GameObject
        public Action Hide;         // how to hide this panel
        public Func<bool> IsActive; // how to check if it’s still visible
    }
    private readonly List<PanelHandle> history = new List<PanelHandle>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        // Iterate backwards through the unified history (true last-opened wins)
        for (int i = history.Count - 1; i >= 0; i--)
        {
            var h = history[i];
            bool active = false;
            
            // Check if panel is active, protected by a try-catch
            try { active = h.IsActive(); } catch { active = false; }

            if (h.key == null)
            {
                // Key is null (panel likely destroyed), remove from history
                history.RemoveAt(i);
                continue;
            }

            if (active)
            {
                // This is the top-most active panel.
                // Store a reference to it *before* hiding.
                var panelToHide = h;

                // Hide it. This 'Hide' delegate might modify the 'history' list
                // (e.g., by calling PushPanel for another panel).
                try { panelToHide.Hide(); } catch { /* swallow errors on hide */ }

                // --- MODIFIED LOGIC ---
                // We only remove the panel from the history IF the 'Hide' call
                // did NOT already manage the stack (e.g., by re-pushing itself or another panel).
                // We check this by seeing if the panel we intended to hide is
                // still at the same index.
                if (history.Count > i && history[i] == panelToHide)
                {
                    // The panel is still here. This means h.Hide() was a simple
                    // "hide" and did not manage the stack. We must remove it.
                    history.RemoveAt(i);
                }
                else
                {
                    // The panel at index 'i' is no longer 'panelToHide'.
                    // This means h.Hide() *did* modify the history.
                    // The stack is already managed, so we do nothing.
                }
                return; // We are done
            }
            else
            {
                // Panel is not active, it's a stale entry, remove it
                history.RemoveAt(i);
            }
        }
    }

    public bool HasActivePanels()
    {
        // Check the unified history for any active panel
        // We iterate backwards to clean up stale entries more efficiently
        for (int i = history.Count - 1; i >= 0; i--)
        {
            var h = history[i];
            bool active = false;

            try { active = h.IsActive(); } catch { active = false; }
            
            if (h.key != null && active)
            {
                return true; // Found an active panel!
            }
            
            // Clean up stale/destroyed entries
            if (h.key == null || !active)
            {
                history.RemoveAt(i);
            }
        }

        return false; // No active panels found
    }
}