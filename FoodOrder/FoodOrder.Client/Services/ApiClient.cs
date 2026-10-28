using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace FoodOrder.Client.Services;

/// <summary>
/// Vékony réteg a HttpClient fölött: sikertelen válasznál ApiException-t dob a szerver magyar hibaüzenetével,
/// 401-es válasznál pedig a bejelentkező oldalra visz.
/// </summary>
public class ApiClient(HttpClient http, CookieAuthenticationStateProvider auth, NavigationManager navigation)
{
    public async Task<T> GetAsync<T>(string url)
    {
        using var response = await SendAsync(() => http.GetAsync(url));
        return await ReadAsync<T>(response);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        using var response = await SendAsync(() => http.PostAsJsonAsync(url, body));
        return await ReadAsync<TResponse>(response);
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string url, TRequest body)
    {
        using var response = await SendAsync(() => http.PatchAsJsonAsync(url, body));
        return await ReadAsync<TResponse>(response);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, "A szerver nem érhető el. Ellenőrizd, hogy fut-e az alkalmazás.");
        }

        if (response.IsSuccessStatusCode)
            return response;

        var exception = await ApiException.FromResponseAsync(response);
        response.Dispose();

        if (exception.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            // A süti lejárt vagy törlődött: kijelentkeztetjük, és a bejelentkezés után visszahozzuk ide.
            auth.SignalUnauthenticated();
            RedirectToLogin();
        }

        throw exception;
    }

    private void RedirectToLogin()
    {
        var current = navigation.ToBaseRelativePath(navigation.Uri);
        if (current.StartsWith("bejelentkezes", StringComparison.OrdinalIgnoreCase))
            return;

        navigation.NavigateTo($"bejelentkezes?returnUrl={Uri.EscapeDataString("/" + current)}");
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>()
        ?? throw new ApiException((int)response.StatusCode, "A szerver üres választ küldött.");
}
