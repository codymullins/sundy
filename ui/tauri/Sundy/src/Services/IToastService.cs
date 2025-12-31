namespace Sundy.Services;

public interface IToastService
{
    event Action? OnChange;
    IReadOnlyList<ToastMessage> Toasts { get; }
    void ShowError(string message, int durationMs = 5000);
    void ShowSuccess(string message, int durationMs = 3000);
    void ShowInfo(string message, int durationMs = 3000);
    void Dismiss(Guid id);
}

public record ToastMessage(Guid Id, string Message, ToastType Type, DateTime ExpiresAt);

public enum ToastType
{
    Info,
    Success,
    Error
}
