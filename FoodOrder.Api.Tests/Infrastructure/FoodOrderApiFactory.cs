using FoodOrder.Application.Ports;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Infrastructure.Notifications;
using FoodOrder.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace FoodOrder.Api.Tests.Infrastructure;

/// <summary>
/// A teljes alkalmazás „Testing” környezetben, memóriabeli SQLite adatbázissal. Tesztosztályonként egy példány
/// (IClassFixture), így minden tesztosztálynak saját adatbázisa van. Az e-mail-küldés fake, az outbox-feldolgozó
/// nem fut a háttérben: a tesztek a <see cref="RunOutboxOnceAsync"/> metódussal indítanak egy kört.
/// </summary>
public sealed class FoodOrderApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // A memóriabeli SQLite adatbázis addig él, amíg ez a kapcsolat nyitva van.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FakeEmailGateway Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Az SMTP-beállítást indításkor ellenőrizzük (ValidateOnStart); levél úgysem megy ki, a küldő fake.
        builder.UseSetting("Smtp:Host", "localhost");

        builder.ConfigureTestServices(services =>
        {
            // SQL Server helyett memóriabeli SQLite.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection, sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            // Valódi SMTP helyett a leveleket rögzítő fake.
            services.RemoveAll<IEmailGateway>();
            services.AddSingleton<IEmailGateway>(Emails);

            // Az outbox-feldolgozó ne fusson a háttérben (a tesztek kézzel futtatják).
            var processor = services.Single(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(OutboxProcessor));
            services.Remove(processor);
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        // A Testing környezetben a Program nem migrál és nem seedel: itt hozzuk létre a sémát és a tesztadatokat.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
        await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    }

    /// <summary>
    /// HTTPS-es tesztkliens (a hitelesítő süti Secure), sütikezeléssel és átirányítás-követés nélkül.
    /// Ha van e-mail-cím, a <c>POST /api/auth/login</c> végponttal bejelentkezik; a sütit a kliens tárolja.
    /// </summary>
    public async Task<HttpClient> CreateClientAsync(string? email = null)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        if (email is not null)
            await client.LoginAsync(email);

        return client;
    }

    /// <summary>Egy outbox-feldolgozási kör, ugyanúgy, ahogy a háttérszolgáltatás futtatná.</summary>
    public Task RunOutboxOnceAsync() =>
        new OutboxProcessor(Services.GetRequiredService<IServiceScopeFactory>(), NullLogger<OutboxProcessor>.Instance)
            .ProcessBatchAsync(CancellationToken.None);

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
