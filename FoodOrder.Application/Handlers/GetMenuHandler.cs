using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

public class GetMenuHandler(IDishRepository repository)
{
    public async Task<List<DishDto>> HandleAsync(bool onlyAvailable, CancellationToken ct)
    {
        var dishes = onlyAvailable
            ? await repository.GetAvailableAsync(ct)
            : await repository.GetAllAsync(ct);

        return dishes.Select(d => d.ToDto()).ToList();
    }
}