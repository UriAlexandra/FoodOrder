using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Notifications;
using FoodOrder.Application.Policies;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Application.Handlers;

/// <summary>
/// Use case: Állapotváltás. Sorrend: látja-e a rendelést (404) → nem változott-e közben (409)
/// → van-e joga hozzá (403) → megengedett-e az átmenet (domain, 409).
/// Az e-mail-értesítés az állapotváltással együtt mentődik (outbox), a valós idejű jelzés a mentés után megy ki.
/// </summary>
public class ChangeOrderStatusHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    IUnitOfWork uow,
    IClock clock,
    ICurrentUser currentUser,
    INotificationOutbox outbox,
    IOrderUpdatePublisher updates,
    ILogger<ChangeOrderStatusHandler> logger)
{
    public async Task<OrderDto> HandleAsync(int id, ChangeOrderStatusRequest request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id, ct);
        if (order is null || !OrderStatusPolicy.CanView(order, currentUser))
            throw new NotFoundException("A rendelés nem található.");

        if (order.Version != request.ExpectedVersion)
        {
            logger.LogInformation(
                "Állapotváltás ütközés miatt elutasítva: #{OrderId}, várt verzió {ExpectedVersion}, aktuális {Version}, felhasználó {UserId}.",
                order.Id, request.ExpectedVersion, order.Version, currentUser.UserId);
            throw new ConflictException("A rendelést közben módosították, frissítsd az oldalt.");
        }

        var oldStatus = order.Status;
        var nextStatus = OrderStatus.FromCode(request.NewStatus);

        if (!OrderStatusPolicy.CanChange(order, nextStatus, currentUser))
        {
            logger.LogWarning(
                "Jogosulatlan állapotváltás: #{OrderId} {OldStatus} → {NewStatus}, felhasználó {UserId}.",
                order.Id, oldStatus.Code, nextStatus.Code, currentUser.UserId);
            throw new ForbiddenException("Ehhez az állapotváltáshoz nincs jogosultságod. Vásárlóként csak a még le nem adott (Leadva állapotú) rendelést mondhatod le.");
        }

        order.ChangeStatus(nextStatus, currentUser.DisplayName(), clock.Now);
        outbox.EnqueueOrderEvent(order, OrderEventType.StatusChanged);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Állapotváltás: #{OrderId} {OldStatus} → {NewStatus}, felhasználó {UserId} ({Role}), új verzió {Version}.",
            order.Id, oldStatus.Code, nextStatus.Code, currentUser.UserId, currentUser.IsStaff ? "Staff" : "Customer", order.Version);

        await updates.PublishOrderChangedAsync(order.Id, order.CustomerId, order.Version);

        var courier = order.CourierId is int courierId
            ? await courierRepository.GetByIdAsync(courierId, ct)
            : null;
        return order.ToDto(courier, currentUser);
    }
}
