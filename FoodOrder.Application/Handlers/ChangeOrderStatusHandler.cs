using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Policies;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

/// <summary>
/// Use case: Állapotváltás. Sorrend: látja-e a rendelést (404) → nem változott-e közben (409)
/// → van-e joga hozzá (403) → megengedett-e az átmenet (domain, 409).
/// </summary>
public class ChangeOrderStatusHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    IUnitOfWork uow,
    IClock clock,
    ICurrentUser currentUser)
{
    public async Task<OrderDto> HandleAsync(int id, ChangeOrderStatusRequest request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id, ct);
        if (order is null || !OrderStatusPolicy.CanView(order, currentUser))
            throw new NotFoundException("A rendelés nem található.");

        if (order.Version != request.ExpectedVersion)
            throw new ConflictException("A rendelést közben módosították, frissítsd az oldalt.");

        var nextStatus = OrderStatus.FromCode(request.NewStatus);

        if (!OrderStatusPolicy.CanChange(order, nextStatus, currentUser))
            throw new ForbiddenException("Ehhez az állapotváltáshoz nincs jogosultságod. Vásárlóként csak a még le nem adott (Leadva állapotú) rendelést mondhatod le.");

        order.ChangeStatus(nextStatus, currentUser.DisplayName(), clock.Now);
        await uow.SaveChangesAsync(ct);

        var courier = order.CourierId is int courierId
            ? await courierRepository.GetByIdAsync(courierId, ct)
            : null;
        return order.ToDto(courier, currentUser);
    }
}
