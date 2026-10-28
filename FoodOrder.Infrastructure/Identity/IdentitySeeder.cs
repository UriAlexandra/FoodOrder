using FoodOrder.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FoodOrder.Infrastructure.Identity;

/// <summary>
/// Fejlesztői tesztfiókok. Csak Development környezetben hívjuk (Program.cs), éles környezetben nem futhat.
/// </summary>
public static class IdentitySeeder
{
    public const string DemoPassword = "Demo123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AppRoles.Customer, AppRoles.Staff })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        await EnsureUserAsync(userManager, "dolgozo@foodorder.hu", "Nagy Péter", AppRoles.Staff);
        await EnsureUserAsync(userManager, "anna@foodorder.hu", "Kiss Anna", AppRoles.Customer);
        await EnsureUserAsync(userManager, "bela@foodorder.hu", "Kovács Béla", AppRoles.Customer);
    }

    private static async Task EnsureUserAsync(UserManager<AppUser> userManager, string email, string fullName, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true, FullName = fullName };
        var result = await userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"A(z) {email} tesztfiók létrehozása sikertelen: {string.Join("; ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
    }
}
