using System.Net;
using Microsoft.AspNetCore.SignalR.Client;

namespace FoodOrder.Client.Services;

/// <summary>
/// Újracsatlakozás korlátlanul: azonnal, 2, 5, 10 másodperc múlva, utána 30 másodpercenként.
/// Ha a szerver 401-et ad (lejárt a bejelentkezés), feladja.
/// </summary>
public sealed class KeepTryingRetryPolicy : IRetryPolicy
{
    private static readonly TimeSpan[] Delays =
        [TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)];

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        if (retryContext.RetryReason is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
            return null;

        return retryContext.PreviousRetryCount < Delays.Length
            ? Delays[retryContext.PreviousRetryCount]
            : TimeSpan.FromSeconds(30);
    }
}
