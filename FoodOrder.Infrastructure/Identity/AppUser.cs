using Microsoft.AspNetCore.Identity;

namespace FoodOrder.Infrastructure.Identity;

/// <summary>Bejelentkezni képes felhasználó. A felhasználónév mindig az e-mail-cím.</summary>
public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
