namespace FoodOrder.Shared.Models;

public class OrderItemDto
{
    public string DishName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}