namespace FoodOrder.Shared.Models;

public class OrderDto
{
    public int Id { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
    public int? CourierId { get; set; }
    public string? CourierName { get; set; }
    public string? CourierPhone { get; set; }
    public decimal Total { get; set; }
    public int Version { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
    public List<StatusOptionDto> AllowedNextStatuses { get; set; } = new();
    public List<StatusHistoryEntryDto> History { get; set; } = new();
}