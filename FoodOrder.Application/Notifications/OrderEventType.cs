namespace FoodOrder.Application.Notifications;

/// <summary>Milyen eseményről megy e-mail a vásárlónak.</summary>
public enum OrderEventType
{
    Placed,
    StatusChanged
}
