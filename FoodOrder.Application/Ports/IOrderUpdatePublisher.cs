namespace FoodOrder.Application.Ports;

/// <summary>
/// Valós idejű jelzés a böngészőknek, hogy egy rendelés megváltozott (a vásárlónak és a dolgozóknak).
/// A sikeres mentés UTÁN kell hívni. Az implementáció nem dob kivételt: hibánál csak naplóz, a kérés nem bukik el.
/// </summary>
public interface IOrderUpdatePublisher
{
    Task PublishOrderChangedAsync(int orderId, string customerId, int version);
}
