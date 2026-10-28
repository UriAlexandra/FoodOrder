namespace FoodOrder.Shared.Models;

public enum UserRole
{
    Customer,
    Staff
}

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