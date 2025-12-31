namespace Sundy.Services;

public class ToastService : IToastService
{
    private readonly List<ToastMessage> _toasts = new();
    private readonly object _lock = new();

    public event Action? OnChange;
    public IReadOnlyList<ToastMessage> Toasts
    {
        get
        {
            lock (_lock)
            {
                // Remove expired toasts
                var now = DateTime.UtcNow;
                _toasts.RemoveAll(t => t.ExpiresAt < now);
                return _toasts.ToList();
            }
        }
    }

    public void ShowError(string message, int durationMs = 5000)
    {
        AddToast(message, ToastType.Error, durationMs);
    }

    public void ShowSuccess(string message, int durationMs = 3000)
    {
        AddToast(message, ToastType.Success, durationMs);
    }

    public void ShowInfo(string message, int durationMs = 3000)
    {
        AddToast(message, ToastType.Info, durationMs);
    }

    public void Dismiss(Guid id)
    {
        lock (_lock)
        {
            _toasts.RemoveAll(t => t.Id == id);
        }
        OnChange?.Invoke();
    }

    private void AddToast(string message, ToastType type, int durationMs)
    {
        var toast = new ToastMessage(
            Guid.NewGuid(),
            message,
            type,
            DateTime.UtcNow.AddMilliseconds(durationMs)
        );

        lock (_lock)
        {
            _toasts.Add(toast);
        }
        OnChange?.Invoke();
    }
}
