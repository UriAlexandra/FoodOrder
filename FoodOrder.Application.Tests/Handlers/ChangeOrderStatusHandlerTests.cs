using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Notifications;
using FoodOrder.Application.Tests.Fakes;
using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Tests.Handlers;

public class ChangeOrderStatusHandlerTests
{
    private readonly TestBed _bed = new();

    private static ChangeOrderStatusRequest To(string status, int expectedVersion) =>
        new() { NewStatus = status, ExpectedVersion = expectedVersion };

    [Fact]
    public async Task Staff_CanWalkTheFullLifecycle_WithCourier()
    {
        var order = await _bed.PlaceAnnasOrderAsync();
        var staff = _bed.ChangeStatus(FakeCurrentUser.Staff);

        foreach (var status in new[] { "Accepted", "Preparing", "ReadyForPickup" })
            await staff.HandleAsync(order.Id, To(status, order.Version), CancellationToken.None);

        await _bed.AssignCourier(FakeCurrentUser.Staff).HandleAsync(order.Id,
            new AssignCourierRequest { CourierId = _bed.Courier.Id, ExpectedVersion = order.Version }, CancellationToken.None);

        await staff.HandleAsync(order.Id, To("OutForDelivery", order.Version), CancellationToken.None);
        var result = await staff.HandleAsync(order.Id, To("Delivered", order.Version), CancellationToken.None);

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal("Delivered", result.Status);
        Assert.True(result.IsFinal);
        Assert.Empty(result.AllowedNextStatuses);
        Assert.Equal("Vincze Gábor", result.CourierName);
        Assert.Equal(7, order.Version);   // 1 (leadás) + 5 állapotváltás + 1 futár

        // Minden állapotváltásról pontosan egy e-mail-esemény, a futár-hozzárendelésről nincs.
        Assert.Equal(new[] { "Accepted", "Preparing", "ReadyForPickup", "OutForDelivery", "Delivered" },
            _bed.Outbox.Entries.Select(e => e.StatusCode));
        Assert.All(_bed.Outbox.Entries, e => Assert.Equal(OrderEventType.StatusChanged, e.Type));

        // Valós idejű jelzés minden változásról (a futárról is), mindig az új verzióval.
        Assert.Equal(new[] { 2, 3, 4, 5, 6, 7 }, _bed.Publisher.Publications.Select(p => p.Version));
    }

    [Fact(DisplayName = "Kézbesített rendelés nem kerülhet vissza előkészítésbe")]
    [Trait("Category", "CriticalRule")]
    public async Task DeliveredOrder_CannotGoBackToPreparing()
    {
        var order = await _bed.AnnasOrderInStatusAsync("Accepted", "Preparing", "ReadyForPickup", "OutForDelivery", "Delivered");

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            _bed.ChangeStatus(FakeCurrentUser.Staff).HandleAsync(order.Id, To("Preparing", order.Version), CancellationToken.None));

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Empty(_bed.Journal.Entries);
    }

    [Fact]
    public async Task OutForDelivery_WithoutCourier_IsRejected()
    {
        var order = await _bed.AnnasOrderInStatusAsync("Accepted", "Preparing", "ReadyForPickup");

        await Assert.ThrowsAsync<DomainException>(() =>
            _bed.ChangeStatus(FakeCurrentUser.Staff).HandleAsync(order.Id, To("OutForDelivery", order.Version), CancellationToken.None));

        Assert.Equal(OrderStatus.ReadyForPickup, order.Status);
    }

    [Fact]
    public async Task Customer_CanCancelOwnPlacedOrder()
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        var result = await _bed.ChangeStatus(FakeCurrentUser.Anna).HandleAsync(order.Id, To("Cancelled", 1), CancellationToken.None);

        Assert.Equal("Cancelled", result.Status);
        var entry = Assert.Single(_bed.Outbox.Entries);
        Assert.Equal("Cancelled", entry.StatusCode);
        Assert.Equal(new Publication(order.Id, "anna-id", 2), Assert.Single(_bed.Publisher.Publications));
    }

    [Fact]
    public async Task Customer_CannotCancelAcceptedOrder()
    {
        var order = await _bed.AnnasOrderInStatusAsync("Accepted");

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _bed.ChangeStatus(FakeCurrentUser.Anna).HandleAsync(order.Id, To("Cancelled", order.Version), CancellationToken.None));

        Assert.Contains("Leadva", ex.Message);
        Assert.Equal(OrderStatus.Accepted, order.Status);
        Assert.Empty(_bed.Journal.Entries);   // nincs mentés, outbox és publikálás
    }

    [Theory]
    [InlineData("Accepted")]
    [InlineData("Preparing")]
    [InlineData("Delivered")]
    public async Task Customer_CannotMakeAnyOtherChange(string status)
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _bed.ChangeStatus(FakeCurrentUser.Anna).HandleAsync(order.Id, To(status, 1), CancellationToken.None));

        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public async Task Customer_CannotSeeOrCancelSomeoneElsesOrder()
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _bed.ChangeStatus(FakeCurrentUser.Bela).HandleAsync(order.Id, To("Cancelled", 1), CancellationToken.None));

        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public async Task StaleExpectedVersion_ThrowsConflict()
    {
        var order = await _bed.AnnasOrderInStatusAsync("Accepted");   // a verzió már 2

        await Assert.ThrowsAsync<ConflictException>(() =>
            _bed.ChangeStatus(FakeCurrentUser.Staff).HandleAsync(order.Id, To("Preparing", expectedVersion: 1), CancellationToken.None));

        Assert.Equal(OrderStatus.Accepted, order.Status);
        Assert.Empty(_bed.Journal.Entries);
    }

    [Fact]
    public async Task SuccessfulChange_EnqueuesOutboxBeforeSave_AndPublishesAfterSave()
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        await _bed.ChangeStatus(FakeCurrentUser.Staff).HandleAsync(order.Id, To("Accepted", 1), CancellationToken.None);

        Assert.Equal(new[] { "outbox:StatusChanged", "save", "publish" }, _bed.Journal.Entries);
        Assert.Equal("Accepted", Assert.Single(_bed.Outbox.Entries).StatusCode);
        Assert.Equal(new Publication(order.Id, "anna-id", 2), Assert.Single(_bed.Publisher.Publications));
    }
}
