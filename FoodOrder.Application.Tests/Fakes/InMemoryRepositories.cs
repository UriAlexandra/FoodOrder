using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Tests.Fakes;

/// <summary>Az EF Core-hoz hasonlóan az azonosítót a „mentés” adja (a privát settert reflexióval állítjuk).</summary>
internal static class EntityIds
{
    public static void Assign(object entity, int id) => entity.GetType().GetProperty("Id")!.SetValue(entity, id);
}

public sealed class InMemoryDishRepository : IDishRepository
{
    private readonly List<Dish> _dishes = new();

    public Dish Add(Dish dish)
    {
        EntityIds.Assign(dish, _dishes.Count + 1);
        _dishes.Add(dish);
        return dish;
    }

    public Task<Dish?> GetByIdAsync(int id, CancellationToken ct) => Task.FromResult(_dishes.FirstOrDefault(d => d.Id == id));
    public Task<List<Dish>> GetAvailableAsync(CancellationToken ct) => Task.FromResult(_dishes.Where(d => d.IsAvailable).ToList());
    public Task<List<Dish>> GetAllAsync(CancellationToken ct) => Task.FromResult(_dishes.ToList());
    public Task AddAsync(Dish dish, CancellationToken ct) { Add(dish); return Task.CompletedTask; }
}

public sealed class InMemoryCourierRepository : ICourierRepository
{
    private readonly List<Courier> _couriers = new();

    public Courier Add(Courier courier)
    {
        EntityIds.Assign(courier, _couriers.Count + 1);
        _couriers.Add(courier);
        return courier;
    }

    public Task<Courier?> GetByIdAsync(int id, CancellationToken ct) => Task.FromResult(_couriers.FirstOrDefault(c => c.Id == id));
    public Task<List<Courier>> GetAllAsync(CancellationToken ct) => Task.FromResult(_couriers.ToList());
    public Task AddAsync(Courier courier, CancellationToken ct) { Add(courier); return Task.CompletedTask; }
}

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();
    private readonly List<Order> _pending = new();

    public IReadOnlyList<Order> Saved => _orders;

    public Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct) => Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    public Task<List<Order>> GetAllAsync(CancellationToken ct) => Task.FromResult(_orders.ToList());
    public Task<List<Order>> GetByCustomerAsync(string customerId, CancellationToken ct) =>
        Task.FromResult(_orders.Where(o => o.CustomerId == customerId).ToList());

    public Task AddAsync(Order order, CancellationToken ct)
    {
        _pending.Add(order);
        return Task.CompletedTask;
    }

    /// <summary>A FakeUnitOfWork hívja mentéskor: az új rendelések ekkor kapnak azonosítót.</summary>
    public void CommitPending()
    {
        foreach (var order in _pending)
        {
            EntityIds.Assign(order, _orders.Count + 1);
            _orders.Add(order);
        }
        _pending.Clear();
    }
}
