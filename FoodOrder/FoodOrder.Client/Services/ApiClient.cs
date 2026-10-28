using System.Net.Http.Json;

namespace FoodOrder.Client.Services;

/// <summary>
/// Vékony réteg a HttpClient fölött: sikertelen válasznál ApiException-t dob a szerver magyar hibaüzenetével.
/// </summary>
public class ApiClient(HttpClient http)
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

    private static async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
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
        throw exception;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>()
        ?? throw new ApiException((int)response.StatusCode, "A szerver üres választ küldött.");
}
