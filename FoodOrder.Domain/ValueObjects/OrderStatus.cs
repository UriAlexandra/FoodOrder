using FoodOrder.Domain.Exceptions;

namespace FoodOrder.Domain.ValueObjects;

public sealed class OrderStatus : IEquatable<OrderStatus>
{
    public string Code { get; }
    public string DisplayName { get; }
    public bool IsFinal { get; }
    private readonly HashSet<string> _allowedNext;

    private OrderStatus(string code, string displayName, bool isFinal, params string[] allowedNext)
    {
        Code = code;
        DisplayName = displayName;
        IsFinal = isFinal;
        _allowedNext = [.. allowedNext];
    }

    public IReadOnlyCollection<string> AllowedNext => _allowedNext;

    public static readonly OrderStatus Placed = new("Placed", "Leadva", false, "Accepted", "Cancelled");
    public static readonly OrderStatus Accepted = new("Accepted", "Elfogadva", false, "Preparing", "Cancelled");
    public static readonly OrderStatus Preparing = new("Preparing", "Készül", false, "ReadyForPickup");
    public static readonly OrderStatus ReadyForPickup = new("ReadyForPickup", "Kiszállításra kész", false, "OutForDelivery");
    public static readonly OrderStatus OutForDelivery = new("OutForDelivery", "Kiszállítás alatt", false, "Delivered");
    public static readonly OrderStatus Delivered = new("Delivered", "Kézbesítve", true);
    public static readonly OrderStatus Cancelled = new("Cancelled", "Lemondva", true);

    public static IReadOnlyCollection<OrderStatus> All { get; } =
        [Placed, Accepted, Preparing, ReadyForPickup, OutForDelivery, Delivered, Cancelled];

    public bool CanTransitionTo(OrderStatus next) => _allowedNext.Contains(next.Code);

    public static OrderStatus FromCode(string code)
    {
        var status = All.FirstOrDefault(s => s.Code == code);
        if (status is null)
            throw new DomainException($"Ismeretlen állapotkód: {code}");
        return status;
    }

    public bool Equals(OrderStatus? other) => other is null ? false : Code == other.Code;
    public override bool Equals(object? obj) => Equals(obj as OrderStatus);
    public override int GetHashCode() => Code.GetHashCode();
    public static bool operator ==(OrderStatus? left, OrderStatus? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(OrderStatus? left, OrderStatus? right) => !(left == right);
}