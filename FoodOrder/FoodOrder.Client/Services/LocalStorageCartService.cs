using FoodOrder.Client.Storage;
using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public class LocalStorageCartService : ICartService
{
    private readonly IKeyValueStorage _storage;
    private List<CartItemDto> _items = new();

    public IReadOnlyCollection<CartItemDto> Items => _items.AsReadOnly();
    public event Action? OnChange;

    public LocalStorageCartService(IKeyValueStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var storedCart = await _storage.GetItemAsync<List<CartItemDto>>(StorageKeys.Cart);
        if (storedCart != null)
        {
            _items = storedCart;
        }
        NotifyStateChanged();
    }

    public async Task AddToCartAsync(DishDto dish, int quantity)
    {
        var existing = _items.FirstOrDefault(c => c.Dish.Id == dish.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItemDto { Dish = dish, Quantity = quantity });
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
                item.Quantity = newQuantity;
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

    private async Task SaveAsync()
    {
        await _storage.SetItemAsync(StorageKeys.Cart, _items);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}