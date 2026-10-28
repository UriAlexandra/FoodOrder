using FoodOrder.Domain.Entities;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // A Money value object mindenhol complex type: az oszlopai a tulajdonos táblájába kerülnek.
        configurationBuilder.ComplexProperties<Money>();

        // Az OrderStatus value object mindenhol szöveges kódként tárolódik (pl. "Placed").
        configurationBuilder.Properties<OrderStatus>()
            .HaveConversion<OrderStatusConverter>()
            .HaveMaxLength(30);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
