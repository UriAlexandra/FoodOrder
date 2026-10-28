using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Client.Services;

public class ApiMenuService(ApiClient api) : IMenuService
{
    private List<DishDto> _dishes = new();

    public IReadOnlyCollection<DishDto> Dishes => _dishes;
    public event Action? OnChange;

    public async Task LoadAsync()
    {
        _dishes = await api.GetAsync<List<DishDto>>("api/dishes");
        OnChange?.Invoke();
    }

    public async Task AddDishAsync(CreateDishRequest request)
    {
        await api.PostAsync<CreateDishRequest, DishDto>("api/dishes", request);
        await LoadAsync();
    }

    public async Task SetAvailabilityAsync(int dishId, bool isAvailable)
    {
        await api.PatchAsync<SetDishAvailabilityRequest, DishDto>(
            $"api/dishes/{dishId}/availability", new SetDishAvailabilityRequest { IsAvailable = isAvailable });
        await LoadAsync();
    }
}
