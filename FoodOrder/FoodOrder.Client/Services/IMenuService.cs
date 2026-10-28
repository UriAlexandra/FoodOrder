using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public interface IMenuService
{
    IReadOnlyCollection<DishDto> Dishes { get; }
    event Action? OnChange;
    Task InitializeAsync();
    Task AddDishAsync(DishDto newDish);
    Task ToggleDishAvailabilityAsync(int dishId);
}