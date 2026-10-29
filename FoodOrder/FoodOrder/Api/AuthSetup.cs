using FoodOrder.Application.Ports;
using FoodOrder.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Api;

public static class AuthSetup
{
    /// <summary>Hitelesítő süti, szerepkör-policy-k és az aktuális felhasználó.</summary>
    public static IServiceCollection AddFoodOrderAuth(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "FoodOrder.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;

            // Teljes oldalbetöltésnél a védett Blazor-oldalak ide irányítanak (mindkettő névtelenül elérhető oldal).
            options.LoginPath = "/bejelentkezes";
            options.AccessDeniedPath = "/hozzaferes-megtagadva";
            options.ReturnUrlParameter = "returnUrl";

            // Az API- és a SignalR-hívásoknál nincs átirányítás: 401 / 403 ProblemDetails válasz megy vissza.
            options.Events.OnRedirectToLogin = context => ApiAwareRedirectAsync(context,
                StatusCodes.Status401Unauthorized, "Bejelentkezés szükséges", "A folytatáshoz jelentkezz be.");
            options.Events.OnRedirectToAccessDenied = context => ApiAwareRedirectAsync(context,
                StatusCodes.Status403Forbidden, "Nincs jogosultság", "Nincs jogosultságod ehhez a művelethez.");
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.StaffOnly, policy => policy.RequireRole(AppRoles.Staff))
            .AddPolicy(AuthPolicies.CustomerOnly, policy => policy.RequireRole(AppRoles.Customer));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }

    /// <summary>API- vagy SignalR-kérés: ezekre nem HTML-oldal, hanem állapotkód a helyes válasz.</summary>
    public static bool IsApiRequest(PathString path) =>
        path.StartsWithSegments("/api") || path.StartsWithSegments("/hubs");

    private static Task ApiAwareRedirectAsync(
        RedirectContext<CookieAuthenticationOptions> context, int statusCode, string title, string detail)
    {
        if (IsApiRequest(context.Request.Path))
        {
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(
                new ProblemDetails { Status = statusCode, Title = title, Detail = detail },
                options: null, contentType: "application/problem+json");
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
}
