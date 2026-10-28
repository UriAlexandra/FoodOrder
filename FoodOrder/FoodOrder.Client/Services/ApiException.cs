using System.Net.Http.Json;

namespace FoodOrder.Client.Services;

/// <summary>
/// Sikertelen API-hívás. Az üzenet a szerver ProblemDetails válaszának magyar szövege,
/// validációs hibánál (400) a mezőnkénti hibák is elérhetők.
/// </summary>
public class ApiException(int statusCode, string message, IReadOnlyDictionary<string, string[]>? errors = null)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public IReadOnlyDictionary<string, string[]> Errors { get; } =
        errors ?? new Dictionary<string, string[]>();

    public bool IsConflict => StatusCode == 409;

    /// <summary>A mezőhibák egy listában (megjelenítéshez).</summary>
    public IReadOnlyList<string> FieldMessages => Errors.Values.SelectMany(m => m).ToList();

    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage response)
    {
        var status = (int)response.StatusCode;

        ProblemResponse? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        }
        catch (Exception)
        {
            // Nem ProblemDetails formátumú válasz (pl. üres törzs) – alapértelmezett üzenetet adunk.
        }

        var message = problem?.Detail
            ?? (problem?.Errors is { Count: > 0 } ? "Kérlek, javítsd a hibás adatokat." : null)
            ?? DefaultMessage(status);

        return new ApiException(status, message, problem?.Errors);
    }

    private static string DefaultMessage(int status) => status switch
    {
        400 => "Hibás kérés.",
        404 => "A kért adat nem található.",
        409 => "Az adat közben megváltozott, frissítsd az oldalt.",
        >= 500 => "Váratlan hiba történt a szerveren.",
        _ => $"A kérés sikertelen (HTTP {status})."
    };

    private sealed class ProblemResponse
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
