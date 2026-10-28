namespace FoodOrder.Application.Ports;

/// <summary>A kérést küldő, bejelentkezett felhasználó (a szerver tölti ki a hitelesítő sütiből).</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    bool IsStaff { get; }
    bool IsCustomer { get; }
}

public static class CurrentUserExtensions
{
    public static string DisplayName(this ICurrentUser user) =>
        user.FullName ?? user.Email ?? "Ismeretlen felhasználó";
}
