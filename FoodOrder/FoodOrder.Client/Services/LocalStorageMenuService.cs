using FoodOrder.Client.Storage;
using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public class LocalStorageMenuService : IMenuService
{
    private readonly IKeyValueStorage _storage;
    private List<DishDto> _dishes = new();

    public IReadOnlyCollection<DishDto> Dishes => _dishes.AsReadOnly();
    public event Action? OnChange;

    public LocalStorageMenuService(IKeyValueStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var storedDishes = await _storage.GetItemAsync<List<DishDto>>(StorageKeys.Dishes);
        if (storedDishes != null && storedDishes.Any())
        {
            _dishes = storedDishes;
        }
        else
        {
            await SeedDishesAsync();
        }
        NotifyStateChanged();
    }

    public async Task AddDishAsync(DishDto newDish)
    {
        newDish.Id = _dishes.Any() ? _dishes.Max(d => d.Id) + 1 : 1;
        _dishes.Add(newDish);
        await SaveAsync();
    }

    public async Task ToggleDishAvailabilityAsync(int dishId)
    {
        var dish = _dishes.FirstOrDefault(d => d.Id == dishId);
        if (dish != null)
        {
            dish.IsAvailable = !dish.IsAvailable;
            await SaveAsync();
        }
    }

    private async Task SaveAsync()
    {
        await _storage.SetItemAsync(StorageKeys.Dishes, _dishes);
        NotifyStateChanged();
    }

    private async Task SeedDishesAsync()
    {
        _dishes = new List<DishDto>
        {
            new() { Id = 1, Name = "Újházi tyúkhúsleves", Description = "Gazdag, ízletes házi leves tésztával és zöldségekkel.", Category = "Soup", Price = 1890, IsAvailable = true },
            new() { Id = 2, Name = "Gulyásleves", Description = "Hagyományos magyar gulyás marhahússal.", Category = "Soup", Price = 2190, IsAvailable = true },
            new() { Id = 3, Name = "Rántott sertéskaraj", Description = "Krumplipürével és tartármártással.", Category = "MainCourse", Price = 3490, IsAvailable = true },
            new() { Id = 4, Name = "Csirkepaprikás", Description = "Házi nokedlivel és tejföllel.", Category = "MainCourse", Price = 3290, IsAvailable = true },
            new() { Id = 5, Name = "Somlói galuska", Description = "Klasszikus magyar desszert csokoládéöntettel.", Category = "Dessert", Price = 1490, IsAvailable = true },
            new() { Id = 6, Name = "Házi limonádé", Description = "Frissítő citrusos ital mentával.", Category = "Drink", Price = 990, IsAvailable = true }
        };
        await SaveAsync();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}