namespace FoodOrder.Shared.Models;

public class StatusHistoryEntryDto
{
    public string Status { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}