using System;

namespace Bookify.Client.Services
{
    public enum NotificationPanelMode
    {
        Sidebar,
        NavBar
    }

    public class NotificationStateService
    {
        public bool IsOpen { get; private set; }
        public NotificationPanelMode Mode { get; private set; } = NotificationPanelMode.Sidebar;
        public event Action? OnChange;

        public void Toggle(NotificationPanelMode mode = NotificationPanelMode.Sidebar)
        {
            if (IsOpen && Mode != mode)
            {
                // If it's already open but we want to switch layouts (rare), just update the mode
                Mode = mode;
            }
            else
            {
                IsOpen = !IsOpen;
                if (IsOpen) Mode = mode;
            }
            NotifyStateChanged();
        }

        public void Open(NotificationPanelMode mode = NotificationPanelMode.Sidebar)
        {
            if (IsOpen && Mode == mode) return;
            IsOpen = true;
            Mode = mode;
            NotifyStateChanged();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
