using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

/// <summary>Use case: Állapotváltás. A megengedett sorrendet a domain (Order.ChangeStatus) ellenőrzi.</summary>
public class ChangeOrderStatusHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    IUnitOfWork uow,
    IClock clock)
{
    public async Task<OrderDto> HandleAsync(int id, ChangeOrderStatusRequest request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("A rendelés nem található.");

        if (order.Version != request.ExpectedVersion)
            throw new ConflictException("A rendelést közben módosították, frissítsd az oldalt.");

        var nextStatus = OrderStatus.FromCode(request.NewStatus);

        // Az 5. hétig nincs bejelentkezés, ezért a módosító neve még nem ismert.
        order.ChangeStatus(nextStatus, "Demó felhasználó", clock.Now);
        await uow.SaveChangesAsync(ct);

        var courier = order.CourierId is int courierId
            ? await courierRepository.GetByIdAsync(courierId, ct)
            : null;
        return order.ToDto(courier);
    }
}
