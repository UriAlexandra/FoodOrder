using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;

namespace FoodOrder.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public string CustomerId { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerEmail { get; private set; }
    public string DeliveryAddress { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public int? CourierId { get; private set; }
    public int Version { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderStatusChange> _statusHistory = new();
    public IReadOnlyCollection<OrderStatusChange> StatusHistory => _statusHistory.AsReadOnly();

    public Money Total => _items.Aggregate(Money.Zero, (sum, item) => sum.Add(item.LineTotal));

    // EF Core ezzel a konstruktorral hozza létre az objektumot adatbázisból betöltéskor.
#pragma warning disable CS8618
    private Order() { }
#pragma warning restore CS8618

    private Order(string customerId, string customerName, string customerEmail, string deliveryAddress, string? note, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(deliveryAddress) || deliveryAddress.Length > 200)
            throw new DomainException("A szállítási cím 1 és 200 karakter között kell legyen.");
        if (note?.Length > 300)
            throw new DomainException("A megjegyzés maximum 300 karakter lehet.");

        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        DeliveryAddress = deliveryAddress;
        Note = note;
        PlacedAt = now;
        Status = OrderStatus.Placed;
        Version = 1;

        _statusHistory.Add(new OrderStatusChange(null, OrderStatus.Placed, now, customerId));
    }

    public static Order Place(
        string customerId, string customerName, string customerEmail, string deliveryAddress, string? note,
        IEnumerable<(Dish Dish, int Quantity)> lines, DateTimeOffset now)
    {
        var order = new Order(customerId, customerName, customerEmail, deliveryAddress, note, now);

        // Ugyanaz az étel egy tétellé vonódik össze. Mentett ételnél (Id > 0) az azonosító dönt,
        // még nem mentett ételnél (Id = 0, pl. tesztben) maga a példány.
        var itemsByDish = new Dictionary<Dish, OrderItem>(ReferenceEqualityComparer.Instance);

        foreach (var (dish, quantity) in lines)
        {
            if (!dish.IsAvailable)
                throw new DomainException($"Az étel jelenleg nem elérhető: {dish.Name}");

            var existingItem = itemsByDish.GetValueOrDefault(dish)
                ?? (dish.Id != 0 ? order._items.FirstOrDefault(i => i.DishId == dish.Id) : null);

            if (existingItem is not null)
            {
                existingItem.AddQuantity(quantity);
                itemsByDish[dish] = existingItem;
            }
            else
            {
                var item = new OrderItem(dish.Id, dish.Name, dish.Price, quantity);
                order._items.Add(item);
                itemsByDish[dish] = item;
            }
        }

        if (order._items.Count == 0)
            throw new DomainException("A rendelésnek legalább egy tételt kell tartalmaznia.");

        return order;
    }

    public void ChangeStatus(OrderStatus next, string changedBy, DateTimeOffset now)
    {
        if (!Status.CanTransitionTo(next))
            throw new InvalidStatusTransitionException($"Nem engedélyezett állapotváltás: {Status.DisplayName} → {next.DisplayName}");

        if (next == OrderStatus.OutForDelivery && !CourierId.HasValue)
            throw new DomainException("Kiszállítás alatt állapotba csak hozzárendelt futárral léphet a rendelés.");

        _statusHistory.Add(new OrderStatusChange(Status, next, now, changedBy));
        Status = next;
        Version++;
    }

    public void AssignCourier(Courier courier)
    {
        if (!courier.IsActive)
            throw new DomainException("Inaktív futár nem rendelhető ki.");

        if (Status != OrderStatus.Accepted && Status != OrderStatus.Preparing && Status != OrderStatus.ReadyForPickup)
            throw new DomainException("Futárt csak Elfogadva, Készül vagy Kiszállításra kész állapotban lehet hozzárendelni.");

        CourierId = courier.Id;
        Version++;
    }
}