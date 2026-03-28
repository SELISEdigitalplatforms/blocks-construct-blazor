namespace Client.State;

public class SidebarState
{
    public bool IsOpen { get; private set; } = true;
    public bool IsMobileOpen { get; private set; }
    public event Action? OnChange;

    public void Toggle()
    {
        IsOpen = !IsOpen;
        NotifyStateChanged();
    }

    public void ToggleMobile()
    {
        IsMobileOpen = !IsMobileOpen;
        NotifyStateChanged();
    }

    public void CloseMobile()
    {
        IsMobileOpen = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
