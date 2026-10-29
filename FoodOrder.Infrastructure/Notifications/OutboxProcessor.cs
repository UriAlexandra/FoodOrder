using FoodOrder.Application.Notifications;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Infrastructure.Notifications;

/// <summary>
/// Háttérszolgáltatás: 5 másodpercenként elküldi a függő értesítéseket (legfeljebb 20-at, a létrehozás sorrendjében).
/// Minden levél után ment, így egy már elküldött levél nem megy ki újra. Sikertelen küldésnél növekvő várakozással
/// legfeljebb 5-ször próbálkozik; a végleg sikertelen bejegyzés nem tartja fel a többit. Kivétel miatt nem áll le.
/// </summary>
public class OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    public const int BatchSize = 20;
    public const int MaxAttempts = 5;

    // Várakozás az 1., 2., 3., 4. sikertelen próbálkozás után (az 5. után feladja).
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10)
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        try
        {
            do
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Pl. nem érhető el az adatbázis: naplózzuk, és a következő körben újra próbáljuk.
                    logger.LogError(ex, "Az értesítések feldolgozása sikertelen, a következő körben újrapróbálom.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Az alkalmazás leáll.
        }
    }

    /// <summary>Egy feldolgozási kör. Teszteléshez publikus.</summary>
    public async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var store = services.GetRequiredService<IOutboxStore>();
        var clock = services.GetRequiredService<IClock>();
        var gateway = services.GetRequiredService<IEmailGateway>();

        var messages = await store.GetDueAsync(clock.Now, MaxAttempts, BatchSize, ct);
        foreach (var message in messages)
        {
            ct.ThrowIfCancellationRequested();

            // Ami már feldolgozott, az soha nem megy ki újra.
            if (message.ProcessedAt is not null)
                continue;

            try
            {
                var email = await RenderAsync(message, services, ct);
                await gateway.SendAsync(email, ct);
                message.MarkProcessed(clock.Now);
                logger.LogInformation("Értesítés elküldve: {Subject} (outbox #{MessageId}).", email.Subject, message.Id);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var delay = RetryDelays[Math.Min(message.Attempts, RetryDelays.Length - 1)];
                message.MarkFailed(ex.Message, clock.Now + delay);
                logger.LogWarning(ex,
                    "Az értesítés küldése sikertelen (outbox #{MessageId}, rendelés #{OrderId}, {Attempts}/{MaxAttempts}. próbálkozás).",
                    message.Id, message.OrderId, message.Attempts, MaxAttempts);
            }

            // Minden levél után mentünk: a sikeres küldés és a ProcessedAt együtt zárul le.
            await store.SaveChangesAsync(ct);
        }
    }

    /// <summary>A levelet küldéskor rendereli, a rendelés (és a futár) aktuális adataival, az esemény idején érvényes állapottal.</summary>
    private static async Task<EmailMessage> RenderAsync(OutboxMessage message, IServiceProvider services, CancellationToken ct)
    {
        var orders = services.GetRequiredService<IOrderRepository>();
        var couriers = services.GetRequiredService<ICourierRepository>();

        var order = await orders.GetByIdWithDetailsAsync(message.OrderId, ct)
            ?? throw new InvalidOperationException($"A(z) #{message.OrderId} rendelés nem található.");
        var courier = order.CourierId is int courierId
            ? await couriers.GetByIdAsync(courierId, ct)
            : null;

        if (string.IsNullOrWhiteSpace(order.CustomerEmail))
            throw new InvalidOperationException($"A(z) #{order.Id} rendeléshez nem tartozik e-mail-cím.");

        return EmailTemplates.Render(order, courier, message.EventType, OrderStatus.FromCode(message.StatusCode));
    }
}
