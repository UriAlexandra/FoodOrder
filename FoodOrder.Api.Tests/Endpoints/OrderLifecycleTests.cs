using System.Net;
using System.Net.Http.Json;
using FoodOrder.Api.Tests.Infrastructure;
using FoodOrder.Shared.Models;

namespace FoodOrder.Api.Tests.Endpoints;

/// <summary>A rendelés teljes életútja és az állapotgép szabályai HTTP-n keresztül.</summary>
public class OrderLifecycleTests(FoodOrderApiFactory factory) : IClassFixture<FoodOrderApiFactory>
{
    private async Task<(HttpClient Anna, HttpClient Staff, int CourierId)> ClientsAsync()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var staff = await factory.CreateClientAsync(TestAccounts.Staff);
        var couriers = await staff.GetFromJsonAsync<List<CourierDto>>("/api/couriers");
        return (anna, staff, couriers!.First(c => c.IsActive).Id);
    }

    /// <summary>Anna rendelése a dolgozó által Kiszállításra kész állapotig léptetve.</summary>
    private static async Task<OrderDto> ReadyForPickupAsync(HttpClient anna, HttpClient staff)
    {
        var id = await anna.PlaceOrderAsync();
        var order = await staff.ChangeStatusOkAsync(id, "Accepted", 1);
        order = await staff.ChangeStatusOkAsync(id, "Preparing", order.Version);
        return await staff.ChangeStatusOkAsync(id, "ReadyForPickup", order.Version);
    }

    [Fact]
    public async Task FullLifecycle_PlacedToDelivered_WithCourier()
    {
        var (anna, staff, courierId) = await ClientsAsync();

        var order = await ReadyForPickupAsync(anna, staff);
        order = await staff.AssignCourierAsync(order.Id, courierId, order.Version);
        order = await staff.ChangeStatusOkAsync(order.Id, "OutForDelivery", order.Version);
        order = await staff.ChangeStatusOkAsync(order.Id, "Delivered", order.Version);

        Assert.Equal("Delivered", order.Status);
        Assert.True(order.IsFinal);
        Assert.Empty(order.AllowedNextStatuses);

        // A vásárló a saját rendelésén a teljes idővonalat látja (legújabb elöl).
        var tracked = await anna.GetOrderAsync(order.Id);
        Assert.Equal(
            new[] { "Delivered", "OutForDelivery", "ReadyForPickup", "Preparing", "Accepted", "Placed" },
            tracked.History.Select(h => h.Status));
        Assert.Equal(courierId, tracked.CourierId);
    }

    [Fact(DisplayName = "Kézbesített rendelés nem kerülhet vissza előkészítésbe")]
    [Trait("Category", "CriticalRule")]
    public async Task DeliveredOrder_CannotGoBackToPreparing_Returns409WithHungarianDetail()
    {
        var (anna, staff, courierId) = await ClientsAsync();
        var order = await ReadyForPickupAsync(anna, staff);
        order = await staff.AssignCourierAsync(order.Id, courierId, order.Version);
        order = await staff.ChangeStatusOkAsync(order.Id, "OutForDelivery", order.Version);
        order = await staff.ChangeStatusOkAsync(order.Id, "Delivered", order.Version);

        var response = await staff.ChangeStatusAsync(order.Id, "Preparing", order.Version);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.ReadProblemAsync();
        Assert.Equal(409, problem.Status);
        Assert.Equal("Érvénytelen állapotváltás", problem.Title);
        Assert.Equal("Nem engedélyezett állapotváltás: Kézbesítve → Készül", problem.Detail);
        Assert.Equal("Delivered", (await staff.GetOrderAsync(order.Id)).Status);
    }

    [Fact]
    public async Task OutForDelivery_WithoutCourier_IsRejected()
    {
        var (anna, staff, _) = await ClientsAsync();
        var order = await ReadyForPickupAsync(anna, staff);

        Assert.DoesNotContain(order.AllowedNextStatuses, s => s.Code == "OutForDelivery");   // a felület nem is kínálja

        var response = await staff.ChangeStatusAsync(order.Id, "OutForDelivery", order.Version);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("futárral", (await response.ReadProblemAsync()).Detail);
        Assert.Equal("ReadyForPickup", (await staff.GetOrderAsync(order.Id)).Status);
    }

    [Fact]
    public async Task SameExpectedVersionTwice_SecondRequestGets409()
    {
        var (anna, staff, _) = await ClientsAsync();
        var id = await anna.PlaceOrderAsync();

        var first = await staff.ChangeStatusAsync(id, "Accepted", expectedVersion: 1);
        var second = await staff.ChangeStatusAsync(id, "Cancelled", expectedVersion: 1);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("Adatütközés", (await second.ReadProblemAsync()).Title);
        Assert.Equal("Accepted", (await staff.GetOrderAsync(id)).Status);
    }
}
