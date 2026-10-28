using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Ports;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class AssignCourierHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    IUnitOfWork uow,
    ICurrentUser currentUser)
{
    public async Task<OrderDto> HandleAsync(int id, AssignCourierRequest request, CancellationToken ct)
    {
        if (!currentUser.IsStaff)
            throw new ForbiddenException("Futárt csak éttermi dolgozó rendelhet hozzá.");

        var order = await orderRepository.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("A rendelés nem található.");

        if (order.Version != request.ExpectedVersion)
            throw new ConflictException("A rendelést közben módosították, frissítsd az oldalt.");

        var courier = await courierRepository.GetByIdAsync(request.CourierId, ct)
            ?? throw new NotFoundException("A futár nem található.");

        order.AssignCourier(courier);
        await uow.SaveChangesAsync(ct);

        return order.ToDto(courier, currentUser);
    }
}
