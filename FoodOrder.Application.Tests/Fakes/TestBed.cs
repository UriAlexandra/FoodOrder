using FoodOrder.Application.Handlers;
using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Requests;
using Microsoft.Extensions.Logging.Abstractions;

namespace FoodOrder.Application.Tests.Fakes;

/// <summary>
/// A use case-ek összerakva kézi fake-ekkel (mock-könyvtár nélkül). Minden teszt saját példányt kap,
/// így a tesztek egymástól függetlenek.
/// </summary>
public sealed class TestBed
{
    public Journal Journal { get; } = new();
    public InMemoryDishRepository Dishes { get; } = new();
    public InMemoryCourierRepository Couriers { get; } = new();
    public InMemoryOrderRepository Orders { get; } = new();
    public FakeClock Clock { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; }
    public RecordingOutbox Outbox { get; }
    public RecordingPublisher Publisher { get; }

    public Dish Soup { get; }
    public Dish MainCourse { get; }
    public Courier Courier { get; }

    public TestBed()
    {
        UnitOfWork = new FakeUnitOfWork(Orders, Journal);
        Outbox = new RecordingOutbox(Journal);
        Publisher = new RecordingPublisher(Journal);

        Soup = Dishes.Add(Dish.Create("Gulyásleves", "Hagyományos", DishCategory.Soup, Money.Huf(2190)));
        MainCourse = Dishes.Add(Dish.Create("Rántott sertéskaraj", "Pürével", DishCategory.MainCourse, Money.Huf(3490)));
        Courier = Couriers.Add(Courier.Create("Vincze Gábor", "+36 20 123 4567"));
    }

    public PlaceOrderHandler PlaceOrder(ICurrentUser user) =>
        new(Dishes, Orders, UnitOfWork, Clock, user, Outbox, Publisher, NullLogger<PlaceOrderHandler>.Instance);

    public ChangeOrderStatusHandler ChangeStatus(ICurrentUser user) =>
        new(Orders, Couriers, UnitOfWork, Clock, user, Outbox, Publisher, NullLogger<ChangeOrderStatusHandler>.Instance);

    public AssignCourierHandler AssignCourier(ICurrentUser user) =>
        new(Orders, Couriers, UnitOfWork, user, Publisher, NullLogger<AssignCourierHandler>.Instance);

    public TrackOrderHandler Track(ICurrentUser user) => new(Orders, Couriers, user);

    /// <summary>Anna lead egy rendelést (2 × leves); a mellékhatás-naplók utána üresek, hogy a teszt csak a sajátját lássa.</summary>
    public async Task<Order> PlaceAnnasOrderAsync()
    {
        var id = await PlaceOrder(FakeCurrentUser.Anna).HandleAsync(Request(Soup.Id, 2), CancellationToken.None);
        ClearRecordings();
        return Orders.Saved.Single(o => o.Id == id);
    }

    /// <summary>Anna rendelése a dolgozó által a megadott állapotig léptetve (szükség esetén futárral).</summary>
    public async Task<Order> AnnasOrderInStatusAsync(params string[] path)
    {
        var order = await PlaceAnnasOrderAsync();
        foreach (var status in path)
        {
            if (status == "OutForDelivery" && order.CourierId is null)
                await AssignCourier(FakeCurrentUser.Staff).HandleAsync(order.Id,
                    new AssignCourierRequest { CourierId = Courier.Id, ExpectedVersion = order.Version }, CancellationToken.None);

            await ChangeStatus(FakeCurrentUser.Staff).HandleAsync(order.Id,
                new ChangeOrderStatusRequest { NewStatus = status, ExpectedVersion = order.Version }, CancellationToken.None);
        }
        ClearRecordings();
        return order;
    }

    public static PlaceOrderRequest Request(int dishId, int quantity) => new()
    {
        DeliveryAddress = "1055 Budapest, Kossuth tér 1.",
        Note = "Kapucsengő 42",
        Items = [new PlaceOrderItemRequest { DishId = dishId, Quantity = quantity }]
    };

    public void ClearRecordings()
    {
        Journal.Entries.Clear();
        Outbox.Entries.Clear();
        Publisher.Publications.Clear();
    }
}
