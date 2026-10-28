using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Persistence.Repositories;

public class DishRepository(AppDbContext db) : IDishRepository
{
    public Task<Dish?> GetByIdAsync(int id, CancellationToken ct) => db.Dishes.FirstOrDefaultAsync(d => d.Id == id, ct);
    public Task<List<Dish>> GetAvailableAsync(CancellationToken ct) => db.Dishes.Where(d => d.IsAvailable).ToListAsync(ct);
    public Task<List<Dish>> GetAllAsync(CancellationToken ct) => db.Dishes.ToListAsync(ct);
    public async Task AddAsync(Dish dish, CancellationToken ct) => await db.Dishes.AddAsync(dish, ct);
}

public class CourierRepository(AppDbContext db) : ICourierRepository
{
    public Task<Courier?> GetByIdAsync(int id, CancellationToken ct) => db.Couriers.FirstOrDefaultAsync(c => c.Id == id, ct);
    public Task<List<Courier>> GetAllAsync(CancellationToken ct) => db.Couriers.ToListAsync(ct);
    public async Task AddAsync(Courier courier, CancellationToken ct) => await db.Couriers.AddAsync(courier, ct);
}

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
        => db.Orders.Include(o => o.Items).Include(o => o.StatusHistory).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<List<Order>> GetAllAsync(CancellationToken ct)
        => db.Orders.Include(o => o.Items).Include(o => o.StatusHistory).ToListAsync(ct);

    public Task<List<Order>> GetByCustomerAsync(string customerId, CancellationToken ct)
        => db.Orders.Where(o => o.CustomerId == customerId)
            .Include(o => o.Items).Include(o => o.StatusHistory).ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct) => await db.Orders.AddAsync(order, ct);
}