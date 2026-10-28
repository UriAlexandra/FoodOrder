using FoodOrder.Client.Storage;
using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public class LocalStorageOrderService : IOrderService
{
    private readonly IKeyValueStorage _storage;
    private List<OrderDto> _orders = new();

    public IReadOnlyCollection<OrderDto> Orders => _orders.AsReadOnly();
    public event Action? OnChange;

    public LocalStorageOrderService(IKeyValueStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var storedOrders = await _storage.GetItemAsync<List<OrderDto>>(StorageKeys.Orders);
        if (storedOrders != null)
        {
            _orders = storedOrders;
        }
        NotifyStateChanged();
    }

    public async Task<string> PlaceOrderAsync(IEnumerable<CartItemDto> cartItems, string address, string? notes)
    {
        var nextId = 1000 + _orders.Count + 1;
        var orderIdStr = $"#{nextId}";

        var orderItems = cartItems.Select(c => new OrderItemDto
        {
            DishName = c.Dish.Name,
            Price = c.Dish.Price,
            Quantity = c.Quantity
        }).ToList();

        var total = orderItems.Sum(x => x.Price * x.Quantity);

        var newOrder = new OrderDto
        {
            Id = nextId,
            CustomerName = "Kiss Anna",
            DeliveryAddress = address,
            Note = notes,
            Items = orderItems,
            Total = total,
            Status = OrderStatusCode.Placed.ToString(),
            StatusDisplayName = "Leadva",
            PlacedAt = DateTimeOffset.Now
        };

        _orders.Insert(0, newOrder);
        await _storage.SetItemAsync(StorageKeys.Orders, _orders);
        NotifyStateChanged();

        return orderIdStr;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}