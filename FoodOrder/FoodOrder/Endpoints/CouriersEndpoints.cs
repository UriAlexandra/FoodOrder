using FoodOrder.Application.Handlers;
using FoodOrder.Api;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Endpoints;

public static class CouriersEndpoints
{
    public static void MapCouriersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/couriers").WithTags("Couriers");

        group.MapGet("/", async (ListCouriersHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(ct)));

        group.MapPost("/", async (CreateCourierRequest req, AddCourierHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(req, ct);
            return Results.Ok();
        }).AddEndpointFilter<ValidationFilter<CreateCourierRequest>>();
    }
}