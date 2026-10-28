using FoodOrder.Shared.Models;

namespace FoodOrder.Client.Services;

public interface ISessionService
{
    UserRole CurrentRole { get; }
    event Action? OnChange;
    Task InitializeAsync();
    Task SetRoleAsync(UserRole role);
}