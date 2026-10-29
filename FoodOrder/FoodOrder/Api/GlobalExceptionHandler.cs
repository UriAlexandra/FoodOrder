using FoodOrder.Application.Exceptions;
using FoodOrder.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Api;

/// <summary>
/// Kivételből egységes válasz: API-kérésre magyar ProblemDetails (400/403/404/409/500),
/// oldalkérésre a magyar nyelvű hibaoldal (wwwroot/hiba.html).
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment) : IExceptionHandler
{
    private const string ErrorPage = "hiba.html";

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var details = exception switch
        {
            InvalidStatusTransitionException e => Problem(StatusCodes.Status409Conflict, "Érvénytelen állapotváltás", e.Message),
            ConflictException e => Problem(StatusCodes.Status409Conflict, "Adatütközés", e.Message),
            NotFoundException e => Problem(StatusCodes.Status404NotFound, "Nem található", e.Message),
            ForbiddenException e => Problem(StatusCodes.Status403Forbidden, "Nincs jogosultság", e.Message),
            DomainException e => Problem(StatusCodes.Status400BadRequest, "Érvénytelen adat", e.Message),
            BadHttpRequestException => Problem(StatusCodes.Status400BadRequest, "Hibás kérés", "A kérés formátuma nem megfelelő."),
            _ => null
        };

        if (details is null)
        {
            logger.LogError(exception, "Váratlan hiba: {Method} {Path}", context.Request.Method, context.Request.Path);
            details = Problem(StatusCodes.Status500InternalServerError, "Belső szerverhiba", "Váratlan hiba történt a kiszolgálón.");
        }

        if (!AuthSetup.IsApiRequest(context.Request.Path) && await TryWriteErrorPageAsync(context, ct))
            return true;

        context.Response.StatusCode = details.Status!.Value;
        await context.Response.WriteAsJsonAsync(details, options: null, contentType: "application/problem+json", ct);
        return true;
    }

    private async Task<bool> TryWriteErrorPageAsync(HttpContext context, CancellationToken ct)
    {
        var page = environment.WebRootFileProvider.GetFileInfo(ErrorPage);
        if (!page.Exists)
            return false;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(page, ct);
        return true;
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
