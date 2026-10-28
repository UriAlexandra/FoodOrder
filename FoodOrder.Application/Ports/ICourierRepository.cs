using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Ports;

public interface ICourierRepository
{
    Task<Courier?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Courier>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Courier courier, CancellationToken ct);
}