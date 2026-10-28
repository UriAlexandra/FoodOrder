using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Policies;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

/// <summary>Use case: Rendelés követése.</summary>
public class TrackOrderHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    ICurrentUser currentUser)
{
    public async Task<OrderDto> HandleAsync(int id, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id, ct);

        // Más vásárló rendelésénél ugyanazt a választ adjuk, mint ha nem létezne.
        if (order is null || !OrderStatusPolicy.CanView(order, currentUser))
            throw new NotFoundException("A rendelés nem található.");

        var courier = order.CourierId is int courierId
            ? await courierRepository.GetByIdAsync(courierId, ct)
            : null;
        return order.ToDto(courier, currentUser);
    }
}
