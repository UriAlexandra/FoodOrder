using System.Security.Claims;
using FoodOrder.Application.Ports;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Shared;

namespace FoodOrder.Api;

/// <summary>Az ICurrentUser a hitelesítő sütiből visszafejtett felhasználóból.</summary>
public class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
    public string? UserId => IsAuthenticated ? Principal!.FindFirstValue(ClaimTypes.NameIdentifier) : null;
    public string? Email => IsAuthenticated ? Principal!.FindFirstValue(ClaimTypes.Email) : null;
    public string? FullName => IsAuthenticated ? Principal!.FindFirstValue(AppClaimTypes.FullName) : null;
    public bool IsStaff => Principal?.IsInRole(AppRoles.Staff) == true;
    public bool IsCustomer => Principal?.IsInRole(AppRoles.Customer) == true;
}
