using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Notifications;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Requests;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Application.Handlers;

/// <summary>
/// Use case: Rendelés leadása. A vásárló adatai a bejelentkezett felhasználóból jönnek.
/// A visszaigazoló e-mail az outboxba kerül, ugyanabban a mentésben, mint a rendelés; a valós idejű jelzés a mentés után megy ki.
/// </summary>
public class PlaceOrderHandler(
    IDishRepository dishRepository,
    IOrderRepository orderRepository,
    IUnitOfWork uow,
    IClock clock,
    ICurrentUser currentUser,
    INotificationOutbox outbox,
    IOrderUpdatePublisher updates,
    ILogger<PlaceOrderHandler> logger)
{
    public async Task<int> HandleAsync(PlaceOrderRequest request, CancellationToken ct)
    {
        if (!currentUser.IsCustomer || currentUser.UserId is null)
        {
            logger.LogWarning("Elutasított rendelésleadás: a(z) {UserId} felhasználó nem vásárló.", currentUser.UserId);
            throw new ForbiddenException("Rendelést csak vásárló adhat le.");
        }

        var lines = new List<(Dish Dish, int Quantity)>();
        foreach (var item in request.Items)
        {
            var dish = await dishRepository.GetByIdAsync(item.DishId, ct)
                ?? throw new NotFoundException($"Az étel nem található (azonosító: {item.DishId}).");
            lines.Add((dish, item.Quantity));
        }

        var order = Order.Place(
            customerId: currentUser.UserId,
            customerName: currentUser.DisplayName(),
            customerEmail: currentUser.Email ?? string.Empty,
            deliveryAddress: request.DeliveryAddress,
            note: request.Note,
            lines: lines,
            now: clock.Now);

        await orderRepository.AddAsync(order, ct);
        outbox.EnqueueOrderEvent(order, OrderEventType.Placed);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Rendelés leadva: #{OrderId}, vásárló {UserId}, {ItemCount} tétel, végösszeg {Total} Ft.",
            order.Id, order.CustomerId, order.Items.Count, order.Total.Amount);

        await updates.PublishOrderChangedAsync(order.Id, order.CustomerId, order.Version);
        return order.Id;
    }
}
