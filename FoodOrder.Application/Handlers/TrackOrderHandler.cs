using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Handlers;

public class TrackOrderHandler(IOrderRepository orderRepo, ICourierRepository courierRepo)
{
    public async Task<OrderDto> HandleAsync(int id, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Rendelés nem található.");
        var courier = order.CourierId.HasValue ? await courierRepo.GetByIdAsync(order.CourierId.Value, ct) : null;

        return order.ToDto(courier);
    }
}