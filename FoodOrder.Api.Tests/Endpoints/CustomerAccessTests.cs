using System.Net;
using System.Net.Http.Json;
using FoodOrder.Api.Tests.Infrastructure;
using FoodOrder.Shared.Models;

namespace FoodOrder.Api.Tests.Endpoints;

/// <summary>A vásárló csak a saját rendeléseit látja, és csak Leadva állapotban mondhatja le.</summary>
public class CustomerAccessTests(FoodOrderApiFactory factory) : IClassFixture<FoodOrderApiFactory>
{
    [Fact]
    public async Task Anna_CannotSeeBelasOrder()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var bela = await factory.CreateClientAsync(TestAccounts.Bela);
        var belasOrder = await bela.PlaceOrderAsync();

        var response = await anna.GetAsync($"/api/orders/{belasOrder}");
        var annasList = await anna.GetFromJsonAsync<List<OrderDto>>("/api/orders");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);   // nem derül ki, hogy létezik
        Assert.DoesNotContain(annasList!, o => o.Id == belasOrder);
    }

    [Fact]
    public async Task Anna_CannotCancelBelasOrder()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var bela = await factory.CreateClientAsync(TestAccounts.Bela);
        var belasOrder = await bela.PlaceOrderAsync();

        var response = await anna.ChangeStatusAsync(belasOrder, "Cancelled", 1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Placed", (await bela.GetOrderAsync(belasOrder)).Status);
    }

    [Fact]
    public async Task Anna_CanCancelOwnPlacedOrder()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var id = await anna.PlaceOrderAsync();

        var order = await anna.ChangeStatusOkAsync(id, "Cancelled", 1);

        Assert.Equal("Cancelled", order.Status);
    }

    [Fact]
    public async Task Anna_CannotCancelAcceptedOrder_Returns403()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var staff = await factory.CreateClientAsync(TestAccounts.Staff);
        var id = await anna.PlaceOrderAsync();
        var accepted = await staff.ChangeStatusOkAsync(id, "Accepted", 1);

        var response = await anna.ChangeStatusAsync(id, "Cancelled", accepted.Version);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("Leadva", (await response.ReadProblemAsync()).Detail);
        Assert.Equal("Accepted", (await anna.GetOrderAsync(id)).Status);
    }

    [Fact]
    public async Task Customer_IsOfferedOnlyCancellation_StaffEveryAllowedStep()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var staff = await factory.CreateClientAsync(TestAccounts.Staff);
        var id = await anna.PlaceOrderAsync();

        var forAnna = await anna.GetOrderAsync(id);
        var forStaff = await staff.GetOrderAsync(id);

        Assert.Equal(new[] { "Cancelled" }, forAnna.AllowedNextStatuses.Select(s => s.Code));
        Assert.Equal(new[] { "Accepted", "Cancelled" }, forStaff.AllowedNextStatuses.Select(s => s.Code).Order());
    }
}
