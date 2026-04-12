using System;
using System.Collections.Generic;

namespace Bookify.Client.Services
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ToastItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string NotificationType { get; set; } = "General";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class NotificationToastService
    {
        private List<ToastItem> _activeToasts = new();
        public IReadOnlyList<ToastItem> ActiveToasts => _activeToasts.AsReadOnly();

        public event Action? OnChange;

        public void Notify(string title, string message, string type = "General")
        {
            var toast = new ToastItem { Title = title, Message = message, NotificationType = type };
            _activeToasts.Add(toast);
            NotifyStateChanged();

            // Auto-dismiss after 5 seconds
            Task.Delay(5000).ContinueWith(_ => Dismiss(toast.Id));
        }

        public void Dismiss(Guid id)
        {
            var toast = _activeToasts.Find(x => x.Id == id);
            if (toast != null)
            {
                _activeToasts.Remove(toast);
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
