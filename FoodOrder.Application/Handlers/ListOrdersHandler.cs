using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

public class ListOrdersHandler(IOrderRepository orderRepo, ICourierRepository courierRepo)
{
    public async Task<List<OrderDto>> HandleAsync(CancellationToken ct)
    {
        var orders = await orderRepo.GetAllAsync(ct);
        var couriers = await courierRepo.GetAllAsync(ct);
        var courierDict = couriers.ToDictionary(c => c.Id);

        return orders.Select(o => o.ToDto(o.CourierId.HasValue && courierDict.TryGetValue(o.CourierId.Value, out var c) ? c : null))
                     .OrderByDescending(o => o.PlacedAt)
                     .ToList();
    }
}