namespace FoodOrder.Shared.Realtime;

/// <summary>
/// Egy rendelés megváltozott. Csak az azonosító és a verzió megy ki: a részleteket a kliens az API-ból kéri le,
/// így a jogosultság-ellenőrzés egy helyen (az API-n) marad.
/// </summary>
public class OrderChangedEvent
{
    public int OrderId { get; set; }
    public int Version { get; set; }
}
