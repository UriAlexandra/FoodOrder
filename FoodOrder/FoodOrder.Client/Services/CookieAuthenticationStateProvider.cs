using System.Net.Http.Json;
using System.Security.Claims;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Components.Authorization;

namespace FoodOrder.Client.Services;

/// <summary>
/// A bejelentkezési állapot a szerver HttpOnly sütijéből: a kliens nem látja a sütit, ezért a
/// GET /api/auth/me válaszából építi fel a felhasználót. Az eredményt eltárolja, és belépéskor,
/// kilépéskor vagy 401-es válasznál értesíti a felületet.
/// </summary>
public class CookieAuthenticationStateProvider(HttpClient http) : AuthenticationStateProvider, IAuthService
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private Task<AuthenticationState>? _cachedState;

    public UserInfoDto? CurrentUser { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        _cachedState ??= LoadStateAsync();

    public async Task<UserInfoDto> LoginAsync(LoginRequest request)
    {
        var user = await PostAsync("api/auth/login", request);
        SetUser(user);
        return user;
    }

    public async Task<UserInfoDto> RegisterAsync(RegisterRequest request)
    {
        var user = await PostAsync("api/auth/register", request);
        SetUser(user);
        return user;
    }

    public async Task LogoutAsync()
    {
        try
        {
            using var response = await http.PostAsync("api/auth/logout", content: null);
        }
        catch (HttpRequestException)
        {
            // Ha a szerver nem érhető el, helyben akkor is kijelentkeztetjük.
        }
        SetUser(null);
    }

    /// <summary>Az ApiClient hívja 401-es válasznál: a süti lejárt vagy törlődött.</summary>
    public void SignalUnauthenticated()
    {
        if (CurrentUser is null && _cachedState is { IsCompleted: true })
            return;
        SetUser(null);
    }

    private async Task<AuthenticationState> LoadStateAsync()
    {
        try
        {
            using var response = await http.GetAsync("api/auth/me");
            CurrentUser = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<UserInfoDto>()
                : null;
        }
        catch (HttpRequestException)
        {
            CurrentUser = null;
        }
        return CreateState(CurrentUser);
    }

    private async Task<UserInfoDto> PostAsync<TRequest>(string url, TRequest request)
    {
        HttpResponseMessage response;
        try
        {
            response = await http.PostAsJsonAsync(url, request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, "A szerver nem érhető el. Ellenőrizd, hogy fut-e az alkalmazás.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw await ApiException.FromResponseAsync(response);

            return await response.Content.ReadFromJsonAsync<UserInfoDto>()
                ?? throw new ApiException((int)response.StatusCode, "A szerver üres választ küldött.");
        }
    }

    private void SetUser(UserInfoDto? user)
    {
        CurrentUser = user;
        _cachedState = Task.FromResult(CreateState(user));
        NotifyAuthenticationStateChanged(_cachedState);
    }

    private static AuthenticationState CreateState(UserInfoDto? user)
    {
        if (user is null)
            return Anonymous;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "FoodOrderCookie")));
    }
}
