using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class PlaceOrderHandler(IDishRepository dishRepo, IOrderRepository orderRepo, IUnitOfWork uow, IClock clock)
{
    public async Task<int> HandleAsync(PlaceOrderRequest request, CancellationToken ct)
    {
        var lines = new List<(Dish Dish, int Quantity)>();
        foreach (var item in request.Items)
        {
            var dish = await dishRepo.GetByIdAsync(item.DishId, ct) ?? throw new NotFoundException($"Étel nem található: {item.DishId}");
            lines.Add((dish, item.Quantity));
        }

        var order = Order.Place(
            customerId: request.CustomerEmail,
            customerName: request.CustomerName,
            customerEmail: request.CustomerEmail,
            deliveryAddress: request.DeliveryAddress,
            note: request.Note,
            lines: lines,
            now: clock.Now);

        await orderRepo.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);

        return order.Id;
    }
}