using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Ports;

public interface IOrderRepository
{
    Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct);
    Task<List<Order>> GetAllAsync(CancellationToken ct);
    Task<List<Order>> GetByCustomerAsync(string customerId, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
}