namespace FoodOrder.Shared.Models;

public class OrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public OrderStatusCode Status { get; set; } = OrderStatusCode.Placed;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}