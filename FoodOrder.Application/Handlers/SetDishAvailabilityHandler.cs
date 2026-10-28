using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class SetDishAvailabilityHandler(IDishRepository repository, IUnitOfWork uow)
{
    public async Task<DishDto> HandleAsync(int id, SetDishAvailabilityRequest request, CancellationToken ct)
    {
        var dish = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Az étel nem található (azonosító: {id}).");

        if (request.IsAvailable) dish.MakeAvailable();
        else dish.MakeUnavailable();

        await uow.SaveChangesAsync(ct);
        return dish.ToDto();
    }
}
