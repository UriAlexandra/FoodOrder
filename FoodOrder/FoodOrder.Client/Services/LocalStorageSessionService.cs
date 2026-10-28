using FoodOrder.Client.Storage;
using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public class LocalStorageSessionService : ISessionService
{
    private readonly IKeyValueStorage _storage;
    public UserRole CurrentRole { get; private set; } = UserRole.Customer;
    public event Action? OnChange;

    public LocalStorageSessionService(IKeyValueStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var roleStr = await _storage.GetItemAsync<string>(StorageKeys.Role);
        if (!string.IsNullOrEmpty(roleStr) && Enum.TryParse<UserRole>(roleStr, out var parsedRole))
        {
            CurrentRole = parsedRole;
        }
        NotifyStateChanged();
    }

    public async Task SetRoleAsync(UserRole role)
    {
        CurrentRole = role;
        await _storage.SetItemAsync(StorageKeys.Role, CurrentRole.ToString());
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}