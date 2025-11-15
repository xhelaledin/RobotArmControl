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
                // This is the top-most active panel, hide it
                try { h.Hide(); } catch { /* swallow errors on hide */ }
                history.RemoveAt(i); // Remove after hiding
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