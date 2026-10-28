using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Client.Services;

public class ApiOrderService(ApiClient api) : IOrderService
{
    public Task<List<OrderDto>> GetOrdersAsync() =>
        api.GetAsync<List<OrderDto>>("api/orders");

    public Task<OrderDto> GetOrderAsync(int orderId) =>
        api.GetAsync<OrderDto>($"api/orders/{orderId}");

    public async Task<int> PlaceOrderAsync(IEnumerable<CartItemDto> cartItems, string deliveryAddress, string? note)
    {
        // A vásárló adatait a szerver a bejelentkezett felhasználóból veszi.
        var request = new PlaceOrderRequest
        {
            DeliveryAddress = deliveryAddress.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Items = cartItems
                .Select(i => new PlaceOrderItemRequest { DishId = i.Dish.Id, Quantity = i.Quantity })
                .ToList()
        };

        var response = await api.PostAsync<PlaceOrderRequest, PlaceOrderResponse>("api/orders", request);
        return response.OrderId;
    }

    public Task<OrderDto> ChangeStatusAsync(int orderId, string newStatus, int expectedVersion) =>
        api.PostAsync<ChangeOrderStatusRequest, OrderDto>(
            $"api/orders/{orderId}/status",
            new ChangeOrderStatusRequest { NewStatus = newStatus, ExpectedVersion = expectedVersion });

    public Task<OrderDto> AssignCourierAsync(int orderId, int courierId, int expectedVersion) =>
        api.PostAsync<AssignCourierRequest, OrderDto>(
            $"api/orders/{orderId}/courier",
            new AssignCourierRequest { CourierId = courierId, ExpectedVersion = expectedVersion });
}
