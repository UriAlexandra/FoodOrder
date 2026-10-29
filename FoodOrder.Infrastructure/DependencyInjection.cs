using FoodOrder.Application.Ports;
using FoodOrder.Infrastructure.Email;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Infrastructure.Notifications;
using FoodOrder.Infrastructure.Persistence;
using FoodOrder.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
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

        // ASP.NET Core Identity: felhasználók és szerepkörök ugyanabban az adatbázisban, sütis bejelentkezéssel.
        services.AddIdentity<AppUser, IdentityRole>(IdentitySetup.ConfigureOptions)
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<HungarianIdentityErrorDescriber>()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDishRepository, DishRepository>();
        services.AddScoped<ICourierRepository, CourierRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // E-mail-értesítés outbox mintával: a handler a rendeléssel együtt menti, a háttérszolgáltatás küldi el.
        services.AddScoped<INotificationOutbox, EfNotificationOutbox>();
        services.AddScoped<IOutboxStore, EfOutboxStore>();
        services.AddScoped<IEmailGateway, MailKitEmailGateway>();
        services.AddOptions<SmtpOptions>()
            .Bind(config.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
