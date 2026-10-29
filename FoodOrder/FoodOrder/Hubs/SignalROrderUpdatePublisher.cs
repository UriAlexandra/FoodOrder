using FoodOrder.Application.Ports;
using FoodOrder.Shared.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace FoodOrder.Hubs;

/// <summary>
/// „OrderChanged” üzenet a rendelés vásárlójának és a dolgozóknak. Csak azonosító és verzió megy ki.
/// Hibánál csak naplóz: a rendelés már mentve van, a kérés nem bukhat el a valós idejű jelzés miatt.
/// </summary>
public class SignalROrderUpdatePublisher(IHubContext<OrderHub> hub, ILogger<SignalROrderUpdatePublisher> logger)
    : IOrderUpdatePublisher
{
    public async Task PublishOrderChangedAsync(int orderId, string customerId, int version)
    {
        try
        {
            await hub.Clients
                .Groups(OrderHubGroups.User(customerId), OrderHubGroups.Staff)
                .SendAsync(OrderHubContract.OrderChanged, new OrderChangedEvent { OrderId = orderId, Version = version });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "A(z) #{OrderId} rendelés valós idejű jelzése sikertelen.", orderId);
        }
    }
}
