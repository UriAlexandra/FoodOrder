using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.ValueObjects;

namespace FoodOrder.Application.Policies;

/// <summary>
/// Ki mit tehet egy rendeléssel. A sorrendet (melyik állapot után mi jöhet) a domain ellenőrzi;
/// ez az osztály csak a szerepkör szerinti jogot dönti el.
/// </summary>
public static class OrderStatusPolicy
{
    /// <summary>Az éttermi dolgozó minden rendelést lát, a vásárló csak a sajátját.</summary>
    public static bool CanView(Order order, ICurrentUser user) => user.IsStaff || IsOwner(order, user);

    /// <summary>
    /// Az éttermi dolgozó bármelyik, a domain által megengedett átmenetet kérheti.
    /// A vásárló csak a saját, még „Leadva” állapotú rendelését mondhatja le.
    /// </summary>
    public static bool CanChange(Order order, OrderStatus next, ICurrentUser user) =>
        user.IsStaff || (next == OrderStatus.Cancelled && CustomerCanCancel(order, user));

    /// <summary>A felhasználónak felkínálható következő állapotok (ebből rajzol gombot a felület).</summary>
    public static IReadOnlyList<OrderStatus> AllowedNextFor(Order order, ICurrentUser user)
    {
        if (user.IsStaff)
            return order.GetAllowedNextStatuses();

        return CustomerCanCancel(order, user) ? [OrderStatus.Cancelled] : [];
    }

    public static bool IsOwner(Order order, ICurrentUser user) =>
        user.UserId is not null && order.CustomerId == user.UserId;

    private static bool CustomerCanCancel(Order order, ICurrentUser user) =>
        user.IsCustomer && IsOwner(order, user) && order.Status == OrderStatus.Placed;
}
