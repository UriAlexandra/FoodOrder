using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

/// <summary>
/// Csak megjelenítés: az idővonal lépései és az állapotjelvények színe.
/// Azt, hogy melyik állapotba lehet lépni, mindig a szerver adja (OrderDto.AllowedNextStatuses).
/// </summary>
public static class OrderStatusUi
{
    public static readonly string Delivered = nameof(OrderStatusCode.Delivered);
    public static readonly string Cancelled = nameof(OrderStatusCode.Cancelled);
    public static readonly string OutForDelivery = nameof(OrderStatusCode.OutForDelivery);

    /// <summary>A rendelés szokásos útja a leadástól a kézbesítésig.</summary>
    public static readonly IReadOnlyList<(string Code, string Label)> Timeline =
    [
        (nameof(OrderStatusCode.Placed), "Leadva"),
        (nameof(OrderStatusCode.Accepted), "Elfogadva"),
        (nameof(OrderStatusCode.Preparing), "Készül"),
        (nameof(OrderStatusCode.ReadyForPickup), "Kiszállításra kész"),
        (nameof(OrderStatusCode.OutForDelivery), "Kiszállítás alatt"),
        (nameof(OrderStatusCode.Delivered), "Kézbesítve")
    ];

    public static string BadgeClass(string statusCode) => statusCode switch
    {
        nameof(OrderStatusCode.Placed) => "text-bg-secondary",
        nameof(OrderStatusCode.Accepted) => "text-bg-info",
        nameof(OrderStatusCode.Preparing) => "text-bg-warning",
        nameof(OrderStatusCode.ReadyForPickup) => "text-bg-primary",
        nameof(OrderStatusCode.OutForDelivery) => "text-bg-primary",
        nameof(OrderStatusCode.Delivered) => "text-bg-success",
        nameof(OrderStatusCode.Cancelled) => "text-bg-danger",
        _ => "text-bg-light"
    };
}
