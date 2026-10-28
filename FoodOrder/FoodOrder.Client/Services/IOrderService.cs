using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public interface IOrderService
{
    IReadOnlyCollection<OrderDto> Orders { get; }
    event Action? OnChange;
    Task InitializeAsync();
    Task<string> PlaceOrderAsync(IEnumerable<CartItemDto> cartItems, string address, string? notes);
}