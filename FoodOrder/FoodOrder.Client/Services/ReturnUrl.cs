namespace FoodOrder.Client.Services;

/// <summary>A bejelentkezés utáni visszatérési cím ellenőrzése (csak az alkalmazáson belüli cím fogadható el).</summary>
public static class ReturnUrl
{
    public static bool IsLocal(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && url.StartsWith('/')
        && !url.StartsWith("//")
        && !url.StartsWith("/\\")
        && !url.Contains("://");

    /// <summary>
    /// Hová menjen a felhasználó bejelentkezés után: a kért oldalra, ha az alkalmazáson belüli és nem
    /// a belépés/regisztráció maga; különben dolgozó a beérkezett rendelésekhez, vásárló az étlapra.
    /// </summary>
    public static string Resolve(string? requested, IEnumerable<string> roles)
    {
        if (IsLocal(requested))
        {
            var path = requested!.TrimStart('/');
            var isAuthPage = path.StartsWith("bejelentkezes", StringComparison.OrdinalIgnoreCase)
                          || path.StartsWith("regisztracio", StringComparison.OrdinalIgnoreCase);
            if (!isAuthPage)
                return path;
        }

        return roles.Contains(FoodOrder.Shared.AppRoles.Staff) ? "etterem/rendelesek" : "";
    }
}
