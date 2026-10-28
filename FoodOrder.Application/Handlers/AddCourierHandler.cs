using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class AddCourierHandler(ICourierRepository repository, IUnitOfWork uow)
{
    public async Task HandleAsync(CreateCourierRequest request, CancellationToken ct)
    {
        var courier = Courier.Create(request.Name, request.Phone);
        await repository.AddAsync(courier, ct);
        await uow.SaveChangesAsync(ct);
    }
}