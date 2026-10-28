using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public interface ICartService
{
    IReadOnlyCollection<CartItemDto> Items { get; }
    event Action? OnChange;
    Task InitializeAsync();
    Task AddToCartAsync(DishDto dish, int quantity);
    Task UpdateCartItemAsync(int dishId, int newQuantity);
    Task RemoveFromCartAsync(int dishId);
    Task ClearCartAsync();
}