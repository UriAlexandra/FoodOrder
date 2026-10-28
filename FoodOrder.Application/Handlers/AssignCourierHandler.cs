using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class AssignCourierHandler(IOrderRepository orderRepo, ICourierRepository courierRepo, IUnitOfWork uow)
{
    public async Task HandleAsync(int id, AssignCourierRequest request, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Rendelés nem található.");
        if (order.Version != request.ExpectedVersion)
            throw new ConflictException("A rendelést közben módosították, frissítsd az oldalt.");

        var courier = await courierRepo.GetByIdAsync(request.CourierId, ct) ?? throw new NotFoundException("Futár nem található.");

        order.AssignCourier(courier);
        await uow.SaveChangesAsync(ct);
    }
}