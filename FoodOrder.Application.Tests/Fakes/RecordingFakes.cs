using FoodOrder.Application.Notifications;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Tests.Fakes;

/// <summary>A mellékhatások sorrendje (outbox, mentés, publikálás) egy közös naplóban.</summary>
public sealed class Journal
{
    public List<string> Entries { get; } = new();
}

public sealed class FakeUnitOfWork(InMemoryOrderRepository orders, Journal journal) : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        orders.CommitPending();
        SaveCount++;
        journal.Entries.Add("save");
        return Task.CompletedTask;
    }
}

public sealed class FakeClock : IClock
{
    public DateTimeOffset Now { get; set; } = new(2026, 10, 28, 18, 0, 0, TimeSpan.FromHours(1));
}

/// <summary>Az outboxba tett esemény a felvétel pillanatában érvényes állapotkóddal.</summary>
public sealed record OutboxEntry(Order Order, OrderEventType Type, string StatusCode);

public sealed class RecordingOutbox(Journal journal) : INotificationOutbox
{
    public List<OutboxEntry> Entries { get; } = new();

    public void EnqueueOrderEvent(Order order, OrderEventType type)
    {
        Entries.Add(new OutboxEntry(order, type, order.Status.Code));
        journal.Entries.Add($"outbox:{type}");
    }
}

public sealed record Publication(int OrderId, string CustomerId, int Version);

public sealed class RecordingPublisher(Journal journal) : IOrderUpdatePublisher
{
    public List<Publication> Publications { get; } = new();

    public Task PublishOrderChangedAsync(int orderId, string customerId, int version)
    {
        Publications.Add(new Publication(orderId, customerId, version));
        journal.Entries.Add("publish");
        return Task.CompletedTask;
    }
}
