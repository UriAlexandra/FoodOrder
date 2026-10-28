using FoodOrder.Api;
using FoodOrder.Application.Handlers;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // ?customerEmail=... : egy vásárló rendelései (az 5. héttől a bejelentkezett felhasználó adja)
        group.MapGet("/", async ([FromQuery] string? customerEmail, ListOrdersHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(customerEmail, ct)));

        group.MapGet("/{id:int}", async (int id, TrackOrderHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, ct)));

        group.MapPost("/", async (PlaceOrderRequest req, PlaceOrderHandler handler, CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(req, ct);
            return Results.Created($"/api/orders/{id}", new PlaceOrderResponse { OrderId = id });
        }).AddEndpointFilter<ValidationFilter<PlaceOrderRequest>>();

        // Állapotváltás és futár-hozzárendelés: a válasz a frissített rendelés,
        // így a felület csak az érintett kártyát cseréli.
        group.MapPost("/{id:int}/status", async (int id, ChangeOrderStatusRequest req, ChangeOrderStatusHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, req, ct)))
            .AddEndpointFilter<ValidationFilter<ChangeOrderStatusRequest>>();

        group.MapPost("/{id:int}/courier", async (int id, AssignCourierRequest req, AssignCourierHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, req, ct)));
    }
}
