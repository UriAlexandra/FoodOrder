using FoodOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Notifications;

public class EfOutboxStore(AppDbContext db) : IOutboxStore
{
    public Task<List<OutboxMessage>> GetDueAsync(DateTimeOffset now, int maxAttempts, int batchSize, CancellationToken ct) =>
        db.OutboxMessages
            .Where(m => m.ProcessedAt == null
                        && m.Attempts < maxAttempts
                        && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Take(batchSize)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
