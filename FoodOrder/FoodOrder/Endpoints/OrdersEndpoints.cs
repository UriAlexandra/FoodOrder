using FoodOrder.Application.Handlers;
using FoodOrder.Api;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (ListOrdersHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(ct)));

        group.MapGet("/{id:int}", async (int id, TrackOrderHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, ct)));

        group.MapPost("/", async (PlaceOrderRequest req, PlaceOrderHandler handler, CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(req, ct);
            return Results.Ok(new { OrderId = id });
        }).AddEndpointFilter<ValidationFilter<PlaceOrderRequest>>();

        group.MapPost("/{id:int}/status", async (int id, ChangeOrderStatusRequest req, ChangeOrderStatusHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(id, req, ct);
            return Results.Ok();
        }).AddEndpointFilter<ValidationFilter<ChangeOrderStatusRequest>>();

        group.MapPost("/{id:int}/courier", async (int id, AssignCourierRequest req, AssignCourierHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(id, req, ct);
            return Results.Ok();
        });
    }
}