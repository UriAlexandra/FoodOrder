using FoodOrder.Domain.Entities;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Infrastructure.Notifications;
using FoodOrder.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FoodOrder.Infrastructure.Persistence;

/// <summary>Az alkalmazás adatai és az Identity táblái (AspNetUsers, AspNetRoles, ...) egy adatbázisban.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // A Money value object mindenhol complex type: az oszlopai a tulajdonos táblájába kerülnek.
        configurationBuilder.ComplexProperties<Money>();

        // Az OrderStatus value object mindenhol szöveges kódként tárolódik (pl. "Placed").
        configurationBuilder.Properties<OrderStatus>()
            .HaveConversion<OrderStatusConverter>()
            .HaveMaxLength(30);

        // Az integrációs tesztek SQLite-ot használnak, ami a DateTimeOffset-et nem tudja rendezni és összehasonlítani:
        // ott számként tároljuk. SQL Serveren (éles és fejlesztői adatbázis) nincs változás.
        if (Database.ProviderName == SqliteProviderName)
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Először az Identity táblái, utána a saját konfigurációk.
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
