using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class SetDishAvailabilityHandler(IDishRepository repository, IUnitOfWork uow)
{
    public async Task HandleAsync(int id, SetDishAvailabilityRequest request, CancellationToken ct)
    {
        var dish = await repository.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Étel nem található (Id: {id})");

        if (request.IsAvailable) dish.MakeAvailable();
        else dish.MakeUnavailable();

        await uow.SaveChangesAsync(ct);
    }
}