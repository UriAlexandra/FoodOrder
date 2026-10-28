using FoodOrder.Application.Ports;
using FoodOrder.Infrastructure.Persistence;
using FoodOrder.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodOrder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Hiányzik a 'DefaultConnection' connection string az appsettings.json fájlból.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure();
                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDishRepository, DishRepository>();
        services.AddScoped<ICourierRepository, CourierRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
