public interface IHideablePanel
{
    void HidePanel();         // Custom hide logic
    bool IsPanelActive();     // Used to check if the panel is currently visible
}

public interface IHideablePanel2
{
    void HidePanel2();         // Custom hide logic for second panel
    bool IsPanelActive2();     // Check if second panel is visible
}

public interface IHideablePanel3
{
    void HidePanel3();         // Custom hide logic for third panel
    bool IsPanelActive3();     // Check if third panel is visible
}
