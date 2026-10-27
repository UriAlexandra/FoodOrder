namespace FoodOrder.Shared.Models;

public class CartItemDto
{
    public DishDto Dish { get; set; } = new();
    public int Quantity { get; set; }
}