namespace FoodOrder.Shared.Models;

public enum OrderStatusCode
{
    Placed,
    Accepted,
    Preparing,
    ReadyForPickup,
    OutForDelivery,
    Delivered,
    Cancelled
}