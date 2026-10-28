using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

public class ListCouriersHandler(ICourierRepository repository)
{
    public async Task<List<CourierDto>> HandleAsync(CancellationToken ct)
    {
        var couriers = await repository.GetAllAsync(ct);
        return couriers.Select(c => c.ToDto()).ToList();
    }
}