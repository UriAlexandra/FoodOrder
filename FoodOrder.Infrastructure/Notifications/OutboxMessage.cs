using FoodOrder.Application.Notifications;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Infrastructure.Notifications;

/// <summary>
/// Egy elküldendő értesítés. A rendeléssel együtt, ugyanabban a mentésben kerül az adatbázisba;
/// az OutboxProcessor küldi el, és jelöli feldolgozottnak.
/// </summary>
public class OutboxMessage
{
    public const int MaxErrorLength = 1000;

    public long Id { get; private set; }

    /// <summary>Új rendelésnél a mentéskor az EF Core tölti ki az Order navigáció alapján.</summary>
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public OrderEventType EventType { get; private set; }

    /// <summary>A rendelés állapotkódja az esemény idején (pl. "OutForDelivery").</summary>
    public string StatusCode { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    /// <summary>Sikertelen küldés után legközelebb ekkortól próbálkozhat újra (növekvő várakozás).</summary>
    public DateTimeOffset? NextAttemptAt { get; private set; }

    // EF Core ezzel a konstruktorral hozza létre az objektumot adatbázisból betöltéskor.
    private OutboxMessage() { }

    public OutboxMessage(Order order, OrderEventType eventType, DateTimeOffset createdAt)
    {
        Order = order;
        OrderId = order.Id;
        EventType = eventType;
        StatusCode = order.Status.Code;
        CreatedAt = createdAt;
    }

    public void MarkProcessed(DateTimeOffset now)
    {
        ProcessedAt = now;
        LastError = null;
        NextAttemptAt = null;
    }

    public void MarkFailed(string error, DateTimeOffset nextAttemptAt)
    {
        Attempts++;
        LastError = error.Length <= MaxErrorLength ? error : error[..MaxErrorLength];
        NextAttemptAt = nextAttemptAt;
    }
}
