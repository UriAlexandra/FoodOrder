using System.Security.Claims;
using FoodOrder.Api;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Shared;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Identity;

namespace FoodOrder.Endpoints;

public static class AuthEndpoints
{
    private const string InvalidLogin = "Hibás e-mail-cím vagy jelszó.";

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // Regisztrálni csak vásárlóként lehet; sikeres regisztráció után be is jelentkeztetjük.
        group.MapPost("/register", async (RegisterRequest req, UserManager<AppUser> users, SignInManager<AppUser> signIn) =>
        {
            var email = req.Email.Trim();
            var user = new AppUser { UserName = email, Email = email, FullName = req.FullName.Trim() };

            // A CreateAsync az első hibás lépésnél megáll (előbb a jelszót nézi), ezért a felhasználói adatokat és a
            // jelszót előbb együtt ellenőrizzük: a foglalt e-mail-cím és a gyenge jelszó egyszerre jelenik meg.
            var errors = await ValidateNewUserAsync(users, user, req.Password);
            if (errors.Count > 0)
                return Results.ValidationProblem(ToFieldErrors(errors), detail: "Kérlek, javítsd a hibás adatokat.");

            var result = await users.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                return Results.ValidationProblem(ToFieldErrors(result.Errors), detail: "Kérlek, javítsd a hibás adatokat.");

            await users.AddToRoleAsync(user, AppRoles.Customer);
            await signIn.SignInAsync(user, isPersistent: false);
            return Results.Ok(await ToUserInfoAsync(users, user));
        }).AddEndpointFilter<ValidationFilter<RegisterRequest>>();

        // Minden sikertelen esetben (nincs ilyen fiók, rossz jelszó, zárolt fiók) ugyanaz az általános üzenet megy vissza,
        // hogy a válaszból ne derüljön ki, létezik-e a fiók. A zárolás (5 hiba után 5 perc) csak a naplóban látszik.
        group.MapPost("/login", async (LoginRequest req, UserManager<AppUser> users, SignInManager<AppUser> signIn, ILoggerFactory loggerFactory) =>
        {
            var user = await users.FindByEmailAsync(req.Email.Trim());
            if (user is null)
                return Unauthorized(InvalidLogin);

            var result = await signIn.PasswordSignInAsync(user, req.Password, req.RememberMe, lockoutOnFailure: true);
            if (result.IsLockedOut)
                loggerFactory.CreateLogger(typeof(AuthEndpoints)).LogWarning(
                    "Sikertelen bejelentkezés, a fiók zárolva: {UserId}, a zárolás vége: {LockoutEnd}.", user.Id, user.LockoutEnd);
            if (!result.Succeeded)
                return Unauthorized(InvalidLogin);

            return Results.Ok(await ToUserInfoAsync(users, user));
        }).AddEndpointFilter<ValidationFilter<LoginRequest>>();

        group.MapPost("/logout", async (SignInManager<AppUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.NoContent();
        });

        group.MapGet("/me", async (ClaimsPrincipal principal, UserManager<AppUser> users) =>
        {
            if (principal.Identity?.IsAuthenticated != true)
                return Unauthorized("Nincs bejelentkezett felhasználó.");

            var user = await users.GetUserAsync(principal);
            return user is null
                ? Unauthorized("Nincs bejelentkezett felhasználó.")
                : Results.Ok(await ToUserInfoAsync(users, user));
        });
    }

    private static IResult Unauthorized(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sikertelen bejelentkezés", detail: detail);

    private static async Task<UserInfoDto> ToUserInfoAsync(UserManager<AppUser> users, AppUser user) => new()
    {
        Email = user.Email ?? string.Empty,
        FullName = user.FullName,
        Roles = (await users.GetRolesAsync(user)).ToList()
    };

    private static async Task<List<IdentityError>> ValidateNewUserAsync(UserManager<AppUser> users, AppUser user, string password)
    {
        var errors = new List<IdentityError>();
        foreach (var validator in users.UserValidators)
            errors.AddRange((await validator.ValidateAsync(users, user)).Errors);
        foreach (var validator in users.PasswordValidators)
            errors.AddRange((await validator.ValidateAsync(users, user, password)).Errors);
        return errors;
    }

    /// <summary>Az Identity hibáit a regisztrációs űrlap mezőihez rendeli (a kliens a mező alatt mutatja).</summary>
    private static Dictionary<string, string[]> ToFieldErrors(IEnumerable<IdentityError> errors) =>
        errors
            .GroupBy(e => e.Code switch
            {
                var code when code.StartsWith("Password") => nameof(RegisterRequest.Password),
                "DuplicateEmail" or "DuplicateUserName" or "InvalidEmail" or "InvalidUserName" => nameof(RegisterRequest.Email),
                _ => string.Empty
            }, e => e.Description)
            .ToDictionary(g => g.Key, g => g.Distinct().ToArray());
}
