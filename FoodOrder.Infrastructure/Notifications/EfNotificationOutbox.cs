using FoodOrder.Application.Notifications;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Persistence;

namespace FoodOrder.Infrastructure.Notifications;

/// <summary>
/// Az eseményt ugyanahhoz a DbContexthez adja, amelyen a rendelés is van: a handler egyetlen SaveChanges-e
/// egy tranzakcióban menti a rendelést és az értesítést.
/// </summary>
public class EfNotificationOutbox(AppDbContext db, IClock clock) : INotificationOutbox
{
    public void EnqueueOrderEvent(Order order, OrderEventType type) =>
        db.OutboxMessages.Add(new OutboxMessage(order, type, clock.Now));
}
