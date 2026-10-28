using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class SetCourierActiveHandler(ICourierRepository repository, IUnitOfWork uow)
{
    public async Task<CourierDto> HandleAsync(int id, SetCourierActiveRequest request, CancellationToken ct)
    {
        var courier = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"A futár nem található (azonosító: {id}).");

        if (request.IsActive) courier.Activate();
        else courier.Deactivate();

        await uow.SaveChangesAsync(ct);
        return courier.ToDto();
    }
}
