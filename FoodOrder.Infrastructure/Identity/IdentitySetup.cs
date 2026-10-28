using Microsoft.AspNetCore.Identity;

namespace FoodOrder.Infrastructure.Identity;

public static class IdentitySetup
{
    /// <summary>Jelszó: min. 8 karakter, kis- és nagybetű, szám. 5 hibás próbálkozás után 5 perc zárolás.</summary>
    public static void ConfigureOptions(IdentityOptions options)
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    }
}
