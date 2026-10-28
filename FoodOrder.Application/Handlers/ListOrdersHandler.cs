using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

public class ListOrdersHandler(IOrderRepository orderRepository, ICourierRepository courierRepository)
{
    /// <param name="customerEmail">
    /// Ha meg van adva, csak ennek a vásárlónak a rendelései. Az 5. héttől ezt a bejelentkezett
    /// felhasználó adja, nem a kérés.
    /// </param>
    public async Task<List<OrderDto>> HandleAsync(string? customerEmail, CancellationToken ct)
    {
        var orders = await orderRepository.GetAllAsync(ct);
        var couriers = (await courierRepository.GetAllAsync(ct)).ToDictionary(c => c.Id);

        return orders
            .Where(o => string.IsNullOrWhiteSpace(customerEmail)
                        || string.Equals(o.CustomerEmail, customerEmail, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(o => o.PlacedAt)
            .Select(o => o.ToDto(o.CourierId is int courierId ? couriers.GetValueOrDefault(courierId) : null))
            .ToList();
    }
}
