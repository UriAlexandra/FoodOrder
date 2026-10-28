using System.Security.Claims;
using FoodOrder.Client.Storage;
using FoodOrder.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FoodOrder.Client.Services;

/// <summary>
/// A kosár a böngészőben marad, a bejelentkezett felhasználó e-mail-címéhez kötve
/// (foodorder.cart.{email}). Be- és kijelentkezéskor a megfelelő kosár töltődik be.
/// </summary>
public class LocalStorageCartService : ICartService, IDisposable
{
    // Egy ételből legfeljebb 20 darab rendelhető (a szerver is ellenőrzi).
    public const int MaxQuantity = 20;

    private readonly IKeyValueStorage _storage;
    private readonly AuthenticationStateProvider _auth;
    private List<CartItemDto> _items = new();
    private string? _key;

    public IReadOnlyCollection<CartItemDto> Items => _items.AsReadOnly();
    public event Action? OnChange;

    public LocalStorageCartService(IKeyValueStorage storage, AuthenticationStateProvider auth)
    {
        _storage = storage;
        _auth = auth;
        _auth.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public async Task InitializeAsync()
    {
        var state = await _auth.GetAuthenticationStateAsync();
        await LoadForAsync(state.User);
    }

    public async Task AddToCartAsync(DishDto dish, int quantity)
    {
        if (_key is null) return; // bejelentkezés nélkül nincs kosár

        var existing = _items.FirstOrDefault(c => c.Dish.Id == dish.Id);
        if (existing != null)
        {
            existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, MaxQuantity);
        }
        else
        {
            _items.Add(new CartItemDto { Dish = dish, Quantity = Math.Clamp(quantity, 1, MaxQuantity) });
        }
        await SaveAsync();
    }

    public async Task UpdateCartItemAsync(int dishId, int newQuantity)
    {
        var item = _items.FirstOrDefault(c => c.Dish.Id == dishId);
        if (item != null)
        {
            if (newQuantity <= 0)
                _items.Remove(item);
            else
                item.Quantity = Math.Min(newQuantity, MaxQuantity);
            await SaveAsync();
        }
    }

    public async Task RemoveFromCartAsync(int dishId)
    {
        _items.RemoveAll(c => c.Dish.Id == dishId);
        await SaveAsync();
    }

    public async Task ClearCartAsync()
    {
        _items.Clear();
        await SaveAsync();
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> stateTask)
    {
        try
        {
            await LoadForAsync((await stateTask).User);
        }
        catch (Exception)
        {
            // A kosár betöltése nem akaszthatja meg a be- vagy kijelentkezést.
        }
    }

    private async Task LoadForAsync(ClaimsPrincipal user)
    {
        var email = user.Identity?.IsAuthenticated == true ? user.FindFirst(ClaimTypes.Email)?.Value : null;
        _key = email is null ? null : StorageKeys.CartFor(email);
        _items = _key is null
            ? new List<CartItemDto>()
            : await _storage.GetItemAsync<List<CartItemDto>>(_key) ?? new List<CartItemDto>();
        NotifyStateChanged();
    }

    private async Task SaveAsync()
    {
        if (_key is not null)
            await _storage.SetItemAsync(_key, _items);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose() => _auth.AuthenticationStateChanged -= OnAuthenticationStateChanged;
}
