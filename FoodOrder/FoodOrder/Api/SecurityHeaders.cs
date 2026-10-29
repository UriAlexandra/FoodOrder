namespace FoodOrder.Api;

/// <summary>
/// Biztonsági HTTP-fejlécek minden válaszra (oldal, API, statikus fájl, hibaválasz):
/// a böngésző nem találgatja a tartalomtípust, az oldal nem ágyazható be más oldal keretébe (clickjacking),
/// és más oldalra navigáláskor a teljes cím nem kerül ki a Referer fejlécben.
/// </summary>
public static class SecurityHeaders
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use((context, next) =>
        {
            // OnStarting: a hibakezelő a válasz törlésekor a fejléceket is törli, ezért közvetlenül küldés előtt állítjuk be.
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                return Task.CompletedTask;
            });
            return next(context);
        });
}
