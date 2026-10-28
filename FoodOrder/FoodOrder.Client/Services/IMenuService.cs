using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Client.Services;

public interface IMenuService
{
    /// <summary>Az utoljára betöltött étlap (elérhető és nem elérhető ételek is).</summary>
    IReadOnlyCollection<DishDto> Dishes { get; }

    event Action? OnChange;

    Task LoadAsync();
    Task AddDishAsync(CreateDishRequest request);
    Task SetAvailabilityAsync(int dishId, bool isAvailable);
}
