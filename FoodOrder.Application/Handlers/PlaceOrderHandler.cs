using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

/// <summary>Use case: Rendelés leadása. A vásárló adatai a bejelentkezett felhasználóból jönnek.</summary>
public class PlaceOrderHandler(
    IDishRepository dishRepository,
    IOrderRepository orderRepository,
    IUnitOfWork uow,
    IClock clock,
    ICurrentUser currentUser)
{
    public async Task<int> HandleAsync(PlaceOrderRequest request, CancellationToken ct)
    {
        if (!currentUser.IsCustomer || currentUser.UserId is null)
            throw new ForbiddenException("Rendelést csak vásárló adhat le.");

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
        await uow.SaveChangesAsync(ct);

        return order.Id;
    }
}
