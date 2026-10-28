using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Ports;

public interface IDishRepository
{
    Task<Dish?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Dish>> GetAvailableAsync(CancellationToken ct);
    Task<List<Dish>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Dish dish, CancellationToken ct);
}