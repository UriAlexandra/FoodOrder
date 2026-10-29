using FoodOrder.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FoodOrder.Hubs;

/// <summary>
/// Valós idejű rendelésértesítések (/hubs/orders). Csak bejelentkezett felhasználó csatlakozhat.
/// A csoporttagságot kizárólag a szerver dönti el a sütiből: a hubnak nincs kliensből hívható metódusa.
/// </summary>
[Authorize]
public class OrderHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // A UserIdentifier a NameIdentifier claim, ugyanaz, mint a rendelés CustomerId-ja.
        if (Context.UserIdentifier is { } userId)
            await Groups.AddToGroupAsync(Context.ConnectionId, OrderHubGroups.User(userId));

        if (Context.User?.IsInRole(AppRoles.Staff) == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, OrderHubGroups.Staff);

        await base.OnConnectedAsync();
    }
}

public static class OrderHubGroups
{
    public const string Staff = "staff";

    public static string User(string userId) => $"user-{userId}";
}
