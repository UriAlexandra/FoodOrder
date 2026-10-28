using FoodOrder.Domain.ValueObjects;

namespace FoodOrder.Domain.Entities;

public class OrderStatusChange
{
    public int Id { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string ChangedBy { get; private set; }

    // EF Core ezzel a konstruktorral hozza létre az objektumot adatbázisból betöltéskor.
#pragma warning disable CS8618
    private OrderStatusChange() { }
#pragma warning restore CS8618

    internal OrderStatusChange(OrderStatus? fromStatus, OrderStatus toStatus, DateTimeOffset changedAt, string changedBy)
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedAt = changedAt;
        ChangedBy = changedBy;
    }
}