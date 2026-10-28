using FoodOrder.Api;
using FoodOrder.Application.Handlers;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Endpoints;

public static class DishesEndpoints
{
    public static void MapDishesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dishes").WithTags("Dishes");

        group.MapGet("/", async ([FromQuery] bool? onlyAvailable, GetMenuHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(onlyAvailable ?? false, ct)));

        group.MapPost("/", async (CreateDishRequest req, AddDishHandler handler, CancellationToken ct) =>
        {
            var dish = await handler.HandleAsync(req, ct);
            return Results.Created($"/api/dishes/{dish.Id}", dish);
        }).AddEndpointFilter<ValidationFilter<CreateDishRequest>>();

        group.MapPatch("/{id:int}/availability", async (int id, SetDishAvailabilityRequest req, SetDishAvailabilityHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, req, ct)));
    }
}
