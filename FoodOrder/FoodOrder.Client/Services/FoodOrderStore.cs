using System.Text.Json;
using Microsoft.JSInterop;
using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public class FoodOrderStore
{
    private readonly IJSRuntime _jsRuntime;

    public bool IsInitialized { get; private set; }
    public UserRole CurrentRole { get; private set; } = UserRole.Customer;

    public List<DishDto> Dishes { get; private set; } = new();
    public List<CartItemDto> Cart { get; private set; } = new();
    public List<OrderDto> Orders { get; private set; } = new();

    public event Action? OnChange;

    public FoodOrderStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        var roleJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "foodorder.role");
        if (roleJson != null && Enum.TryParse<UserRole>(roleJson, out var parsedRole))
            CurrentRole = parsedRole;

        var dishesJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "foodorder.dishes");
        if (dishesJson != null)
            Dishes = JsonSerializer.Deserialize<List<DishDto>>(dishesJson) ?? new();

        if (!Dishes.Any())
            await SeedDishesAsync();

        var cartJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "foodorder.cart");
        if (cartJson != null)
            Cart = JsonSerializer.Deserialize<List<CartItemDto>>(cartJson) ?? new();

        var ordersJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "foodorder.orders");
        if (ordersJson != null)
            Orders = JsonSerializer.Deserialize<List<OrderDto>>(ordersJson) ?? new();

        IsInitialized = true;
        NotifyStateChanged();
    }

    public async Task SetRoleAsync(UserRole role)
    {
        CurrentRole = role;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "foodorder.role", CurrentRole.ToString());
        NotifyStateChanged();
    }

    // --- CART ---
    public async Task AddToCartAsync(DishDto dish, int quantity)
    {
        var existing = Cart.FirstOrDefault(c => c.Dish.Id == dish.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            Cart.Add(new CartItemDto { Dish = dish, Quantity = quantity });
        }
        await SaveCartAsync();
    }

    public async Task UpdateCartItemAsync(Guid dishId, int newQuantity)
    {
        var item = Cart.FirstOrDefault(c => c.Dish.Id == dishId);
        if (item != null)
        {
            if (newQuantity <= 0)
                Cart.Remove(item);
            else
                item.Quantity = newQuantity;
            await SaveCartAsync();
        }
    }

    public async Task RemoveFromCartAsync(Guid dishId)
    {
        Cart.RemoveAll(c => c.Dish.Id == dishId);
        await SaveCartAsync();
    }

    public async Task ClearCartAsync()
    {
        Cart.Clear();
        await SaveCartAsync();
    }

    private async Task SaveCartAsync()
    {
        var json = JsonSerializer.Serialize(Cart);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "foodorder.cart", json);
        NotifyStateChanged();
    }

    // --- ORDERS ---
    public async Task<string> PlaceOrderAsync(string address, string? notes)
    {
        var nextId = 1000 + Orders.Count + 1;
        var orderIdStr = $"#{nextId}";

        var orderItems = Cart.Select(c => new OrderItemDto
        {
            DishName = c.Dish.Name,
            Price = c.Dish.Price,
            Quantity = c.Quantity
        }).ToList();

        var total = orderItems.Sum(x => x.Price * x.Quantity);

        var newOrder = new OrderDto
        {
            OrderId = orderIdStr,
            CustomerName = "Kiss Anna", // Mock felhasználónév
            DeliveryAddress = address,
            Notes = notes,
            Items = orderItems,
            TotalPrice = total,
            Status = OrderStatusCode.Placed,
            CreatedAt = DateTime.Now
        };

        Orders.Insert(0, newOrder); // Legújabb elöl
        var json = JsonSerializer.Serialize(Orders);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "foodorder.orders", json);

        await ClearCartAsync();
        return orderIdStr;
    }

    // --- MENU MANAGEMENT ---
    public async Task AddDishAsync(DishDto newDish)
    {
        Dishes.Add(newDish);
        await SaveDishesAsync();
    }

    public async Task ToggleDishAvailabilityAsync(Guid dishId)
    {
        var dish = Dishes.FirstOrDefault(d => d.Id == dishId);
        if (dish != null)
        {
            dish.IsAvailable = !dish.IsAvailable;
            await SaveDishesAsync();
        }
    }

    private async Task SaveDishesAsync()
    {
        var json = JsonSerializer.Serialize(Dishes);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "foodorder.dishes", json);
        NotifyStateChanged();
    }

    private async Task SeedDishesAsync()
    {
        Dishes = new List<DishDto>
        {
            new() { Name = "Újházi tyúkhúsleves", Description = "Gazdag, ízletes házi leves tésztával és zöldségekkel.", Category = "Leves", Price = 1890, IsAvailable = true },
            new() { Name = "Gulyásleves", Description = "Hagyományos magyar gulyás marhahússal.", Category = "Leves", Price = 2190, IsAvailable = true },
            new() { Name = "Rántott sertéskaraj", Description = "Krumplipürével és tartármártással.", Category = "Főétel", Price = 3490, IsAvailable = true },
            new() { Name = "Csirkepaprikás", Description = "Házi nokedlivel és tejföllel.", Category = "Főétel", Price = 3290, IsAvailable = true },
            new() { Name = "Somlói galuska", Description = "Klasszikus magyar desszert csokoládéöntettel.", Category = "Desszert", Price = 1490, IsAvailable = true },
            new() { Name = "Házi limonádé", Description = "Frissítő citrusos ital mentával (0.5L).", Category = "Ital", Price = 990, IsAvailable = true }
        };
        await SaveDishesAsync();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}