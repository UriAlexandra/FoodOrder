using FoodOrder.Api;
using FoodOrder.Application.Handlers;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Endpoints;

public static class CouriersEndpoints
{
    public static void MapCouriersEndpoints(this IEndpointRouteBuilder app)
    {
        // Futárokat csak éttermi dolgozó kezelhet.
        var group = app.MapGroup("/api/couriers").WithTags("Couriers")
            .RequireAuthorization(AuthPolicies.StaffOnly);

        group.MapGet("/", async (ListCouriersHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(ct)));

        group.MapPost("/", async (CreateCourierRequest req, AddCourierHandler handler, CancellationToken ct) =>
        {
            var courier = await handler.HandleAsync(req, ct);
            return Results.Created($"/api/couriers/{courier.Id}", courier);
        }).AddEndpointFilter<ValidationFilter<CreateCourierRequest>>();

        group.MapPatch("/{id:int}/active", async (int id, SetCourierActiveRequest req, SetCourierActiveHandler handler, CancellationToken ct)
            => Results.Ok(await handler.HandleAsync(id, req, ct)));
    }
}
