namespace Bookify.Client.Services;

// ── Enums & Records ──────────────────────────────────────────────────────────

public enum ToastLevel { Success, Error, Warning, Info }

public record ToastMessage(Guid Id, ToastLevel Level, string Text);

// ── Service ──────────────────────────────────────────────────────────────────

/// <summary>
/// Singleton service that raises events to display toast notifications.
/// Inject into any Blazor component or service that needs to notify the user.
/// </summary>
public class ToastService
{
    /// <summary>Fired whenever a new toast should be displayed.</summary>
    public event Action<ToastMessage>? OnShow;

    public void ShowSuccess(string message)
        => OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), ToastLevel.Success, message));

    public void ShowError(string message)
        => OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), ToastLevel.Error, message));

    public void ShowWarning(string message)
        => OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), ToastLevel.Warning, message));

    public void ShowInfo(string message)
        => OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), ToastLevel.Info, message));
}
