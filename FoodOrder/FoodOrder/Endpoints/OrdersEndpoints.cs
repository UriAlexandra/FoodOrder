using FoodOrder.Api;
using FoodOrder.Application.Handlers;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        // Minden rendelésvégpont bejelentkezéshez kötött; a szerepkör szerinti szűrést az Application réteg végzi.
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        // Dolgozó: minden rendelés; vásárló: csak a sajátjai.
        group.MapGet("/", async (ListOrdersHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(ct)));

        group.MapGet("/{id:int}", async (int id, TrackOrderHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, ct)));

        group.MapPost("/", async (PlaceOrderRequest req, PlaceOrderHandler handler, CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(req, ct);
            return Results.Created($"/api/orders/{id}", new PlaceOrderResponse { OrderId = id });
        }).AddEndpointFilter<ValidationFilter<PlaceOrderRequest>>()
          .RequireAuthorization(AuthPolicies.CustomerOnly);

        // Állapotváltás és futár-hozzárendelés: a válasz a frissített rendelés,
        // így a felület csak az érintett kártyát cseréli.
        group.MapPost("/{id:int}/status", async (int id, ChangeOrderStatusRequest req, ChangeOrderStatusHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, req, ct)))
            .AddEndpointFilter<ValidationFilter<ChangeOrderStatusRequest>>();

        group.MapPost("/{id:int}/courier", async (int id, AssignCourierRequest req, AssignCourierHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, req, ct)))
            .RequireAuthorization(AuthPolicies.StaffOnly);
    }
}
