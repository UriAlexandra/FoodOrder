using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public interface IOrderService
{
    /// <summary>Dolgozónak minden rendelés, vásárlónak csak a sajátjai (a szerver szűr).</summary>
    Task<List<OrderDto>> GetOrdersAsync();
    Task<OrderDto> GetOrderAsync(int orderId);

    /// <returns>Az új rendelés azonosítója.</returns>
    Task<int> PlaceOrderAsync(IEnumerable<CartItemDto> cartItems, string deliveryAddress, string? note);

    /// <returns>A frissített rendelés.</returns>
    Task<OrderDto> ChangeStatusAsync(int orderId, string newStatus, int expectedVersion);

    /// <returns>A frissített rendelés.</returns>
    Task<OrderDto> AssignCourierAsync(int orderId, int courierId, int expectedVersion);
}
