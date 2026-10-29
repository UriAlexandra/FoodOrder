using FoodOrder.Application.Notifications;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Ports;

/// <summary>
/// Outbox: az értesítést a rendeléssel együtt, ugyanabban a mentésben rögzítjük, a levelet pedig egy háttérfolyamat
/// küldi el. Így nem lehet rendelés értesítés nélkül, és értesítés rendelés nélkül.
/// </summary>
public interface INotificationOutbox
{
    /// <summary>Felveszi az eseményt a rendelés jelenlegi állapotkódjával. A következő SaveChanges menti.</summary>
    void EnqueueOrderEvent(Order order, OrderEventType type);
}
