namespace FoodOrder.Client.Services;

public enum ToastKind
{
    Info,
    Success,
    Warning
}

public sealed record ToastMessage(Guid Id, string Text, ToastKind Kind);

/// <summary>Felugró értesítések (toast). A ToastHost jeleníti meg őket egy aria-live régióban; 8 másodperc után eltűnnek.</summary>
public class ToastService
{
    public static readonly TimeSpan DisplayTime = TimeSpan.FromSeconds(8);
    private const int MaxVisible = 4;

    private readonly List<ToastMessage> _messages = new();
    private readonly object _lock = new();

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> Messages
    {
        get
        {
            lock (_lock) return _messages.ToList();
        }
    }

    public void Show(string text, ToastKind kind = ToastKind.Info)
    {
        var toast = new ToastMessage(Guid.NewGuid(), text, kind);
        lock (_lock)
        {
            _messages.Add(toast);
            if (_messages.Count > MaxVisible)
                _messages.RemoveAt(0);
        }
        OnChange?.Invoke();
        _ = DismissLaterAsync(toast.Id);
    }

    public void Dismiss(Guid id)
    {
        bool removed;
        lock (_lock) removed = _messages.RemoveAll(m => m.Id == id) > 0;
        if (removed)
            OnChange?.Invoke();
    }

    private async Task DismissLaterAsync(Guid id)
    {
        await Task.Delay(DisplayTime);
        Dismiss(id);
    }
}
