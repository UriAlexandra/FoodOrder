using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Tests.Fakes;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Tests.Handlers;

public class TrackOrderHandlerTests
{
    private readonly TestBed _bed = new();

    [Fact]
    public async Task OtherCustomersOrder_ThrowsNotFound()
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _bed.Track(FakeCurrentUser.Bela).HandleAsync(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task UnknownOrder_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _bed.Track(FakeCurrentUser.Staff).HandleAsync(42, CancellationToken.None));
    }

    [Fact]
    public async Task Owner_OfPlacedOrder_IsOfferedOnlyCancellation()
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        var dto = await _bed.Track(FakeCurrentUser.Anna).HandleAsync(order.Id, CancellationToken.None);

        Assert.Equal(new[] { "Cancelled" }, dto.AllowedNextStatuses.Select(s => s.Code));
        Assert.False(dto.CanAssignCourier);
    }

    [Fact]
    public async Task Staff_OfPlacedOrder_IsOfferedEveryAllowedTransition()
    {
        var order = await _bed.PlaceAnnasOrderAsync();

        var dto = await _bed.Track(FakeCurrentUser.Staff).HandleAsync(order.Id, CancellationToken.None);

        Assert.Equal(new[] { "Accepted", "Cancelled" }, dto.AllowedNextStatuses.Select(s => s.Code).Order());
        Assert.Equal("Kiss Anna", dto.CustomerName);
    }

    [Fact]
    public async Task Owner_OfAcceptedOrder_IsOfferedNothing_StaffCanAssignCourier()
    {
        var order = await _bed.AnnasOrderInStatusAsync("Accepted");

        var forAnna = await _bed.Track(FakeCurrentUser.Anna).HandleAsync(order.Id, CancellationToken.None);
        var forStaff = await _bed.Track(FakeCurrentUser.Staff).HandleAsync(order.Id, CancellationToken.None);

        Assert.Empty(forAnna.AllowedNextStatuses);
        Assert.False(forAnna.CanAssignCourier);
        Assert.Equal(new[] { "Cancelled", "Preparing" }, forStaff.AllowedNextStatuses.Select(s => s.Code).Order());
        Assert.True(forStaff.CanAssignCourier);
    }

    [Fact]
    public async Task ReadyForPickup_OffersOutForDeliveryOnlyWithCourier()
    {
        var order = await _bed.AnnasOrderInStatusAsync("Accepted", "Preparing", "ReadyForPickup");
        var track = _bed.Track(FakeCurrentUser.Staff);

        var withoutCourier = await track.HandleAsync(order.Id, CancellationToken.None);
        await _bed.AssignCourier(FakeCurrentUser.Staff).HandleAsync(order.Id,
            new AssignCourierRequest { CourierId = _bed.Courier.Id, ExpectedVersion = order.Version }, CancellationToken.None);
        var withCourier = await track.HandleAsync(order.Id, CancellationToken.None);

        Assert.Empty(withoutCourier.AllowedNextStatuses);
        Assert.Equal(new[] { "OutForDelivery" }, withCourier.AllowedNextStatuses.Select(s => s.Code));
        Assert.Equal("Vincze Gábor", withCourier.CourierName);
    }
}
