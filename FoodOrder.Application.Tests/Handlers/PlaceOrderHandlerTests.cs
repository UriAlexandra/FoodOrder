using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Notifications;
using FoodOrder.Application.Tests.Fakes;
using FoodOrder.Domain.Exceptions;

namespace FoodOrder.Application.Tests.Handlers;

public class PlaceOrderHandlerTests
{
    private readonly TestBed _bed = new();

    [Fact]
    public async Task UnavailableDish_IsRejected_AndNothingIsSaved()
    {
        _bed.Soup.MakeUnavailable();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _bed.PlaceOrder(FakeCurrentUser.Anna).HandleAsync(TestBed.Request(_bed.Soup.Id, 1), CancellationToken.None));

        Assert.Contains("nem rendelhető", ex.Message);
        Assert.Empty(_bed.Orders.Saved);
        Assert.Empty(_bed.Journal.Entries);
    }

    [Fact]
    public async Task UnknownDish_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _bed.PlaceOrder(FakeCurrentUser.Anna).HandleAsync(TestBed.Request(dishId: 999, quantity: 1), CancellationToken.None));

        Assert.Empty(_bed.Orders.Saved);
        Assert.Empty(_bed.Outbox.Entries);
        Assert.Empty(_bed.Publisher.Publications);
    }

    [Fact]
    public async Task CustomerData_ComesFromCurrentUser()
    {
        var id = await _bed.PlaceOrder(FakeCurrentUser.Anna).HandleAsync(TestBed.Request(_bed.Soup.Id, 2), CancellationToken.None);

        var order = Assert.Single(_bed.Orders.Saved);
        Assert.Equal(id, order.Id);
        Assert.Equal("anna-id", order.CustomerId);
        Assert.Equal("Kiss Anna", order.CustomerName);
        Assert.Equal("anna@foodorder.hu", order.CustomerEmail);
        Assert.Equal(4380m, order.Total.Amount);
    }

    [Fact]
    public async Task Staff_CannotPlaceOrder()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _bed.PlaceOrder(FakeCurrentUser.Staff).HandleAsync(TestBed.Request(_bed.Soup.Id, 1), CancellationToken.None));

        Assert.Empty(_bed.Orders.Saved);
    }

    [Fact]
    public async Task SuccessfulOrder_WritesOutboxEventInTheSameSave_ThenPublishes()
    {
        var id = await _bed.PlaceOrder(FakeCurrentUser.Anna).HandleAsync(TestBed.Request(_bed.Soup.Id, 1), CancellationToken.None);

        var entry = Assert.Single(_bed.Outbox.Entries);
        Assert.Equal(OrderEventType.Placed, entry.Type);
        Assert.Equal("Placed", entry.StatusCode);
        Assert.Equal(id, entry.Order.Id);   // a mentéskor kapott azonosító az outbox-bejegyzés rendelésén is megvan

        Assert.Equal(new Publication(id, "anna-id", 1), Assert.Single(_bed.Publisher.Publications));

        // Az outbox-bejegyzés a mentés ELŐTT kerül a változások közé, a valós idejű jelzés a mentés UTÁN megy ki.
        Assert.Equal(new[] { "outbox:Placed", "save", "publish" }, _bed.Journal.Entries);
        Assert.Equal(1, _bed.UnitOfWork.SaveCount);
    }
}
