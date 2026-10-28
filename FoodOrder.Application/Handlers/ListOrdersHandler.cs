using FoodOrder.Application.Mappings;
using FoodOrder.Application.Policies;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

/// <summary>Az éttermi dolgozó minden rendelést lát, a vásárló csak a sajátjait.</summary>
public class ListOrdersHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    ICurrentUser currentUser)
{
    public async Task<List<OrderDto>> HandleAsync(CancellationToken ct)
    {
        // A szűrés az adatbázisban történik; a policy-ellenőrzés csak biztonsági háló.
        var orders = currentUser switch
        {
            { IsStaff: true } => await orderRepository.GetAllAsync(ct),
            { IsCustomer: true, UserId: { } userId } => await orderRepository.GetByCustomerAsync(userId, ct),
            _ => []
        };
        var couriers = (await courierRepository.GetAllAsync(ct)).ToDictionary(c => c.Id);

        return orders
            .Where(o => OrderStatusPolicy.CanView(o, currentUser))
            .OrderByDescending(o => o.PlacedAt)
            .Select(o => o.ToDto(o.CourierId is int courierId ? couriers.GetValueOrDefault(courierId) : null, currentUser))
            .ToList();
    }
}
