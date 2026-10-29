namespace FoodOrder.Infrastructure.Notifications;

/// <summary>Az OutboxProcessor adatelérése (EF Core-ral az EfOutboxStore valósítja meg).</summary>
public interface IOutboxStore
{
    /// <summary>A még fel nem dolgozott, újrapróbálható bejegyzések a létrehozás sorrendjében.</summary>
    Task<List<OutboxMessage>> GetDueAsync(DateTimeOffset now, int maxAttempts, int batchSize, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
