using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class ChangeOrderStatusHandler(IOrderRepository repository, IUnitOfWork uow, IClock clock)
{
    public async Task HandleAsync(int id, ChangeOrderStatusRequest request, CancellationToken ct)
    {
        var order = await repository.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Rendelés nem található.");

        if (order.Version != request.ExpectedVersion)
            throw new ConflictException("A rendelést közben módosították, frissítsd az oldalt.");

        var nextStatus = OrderStatus.FromCode(request.NewStatus);
        order.ChangeStatus(nextStatus, "StaffUser", clock.Now);

        await uow.SaveChangesAsync(ct);
    }
}