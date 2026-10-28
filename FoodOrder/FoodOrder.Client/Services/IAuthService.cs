using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Client.Services;

public interface IAuthService
{
    /// <summary>A bejelentkezett felhasználó, vagy null.</summary>
    UserInfoDto? CurrentUser { get; }

    Task<UserInfoDto> LoginAsync(LoginRequest request);
    Task<UserInfoDto> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}
