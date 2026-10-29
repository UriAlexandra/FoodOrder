using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;
using Xunit;

namespace FoodOrder.Domain.Tests.Entities;

public class OrderTests
{
    private Dish CreateValidDish() => Dish.Create("Pizza", "Finom", DishCategory.MainCourse, Money.Huf(2500));
    private Courier CreateValidCourier() => Courier.Create("Futár", "+36 30 123 4567");

    [Fact]
    public void Place_ValidLines_CreatesOrderAndCalculatesTotal()
    {
        var dish1 = CreateValidDish();
        var dish2 = Dish.Create("Leves", "Finom", DishCategory.Soup, Money.Huf(1000));

        var lines = new List<(Dish, int)> { (dish1, 2), (dish2, 1) };
        var order = Order.Place("user1", "Vevő", "vevo@email.com", "Cím", null, lines, DateTimeOffset.Now);

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(6000, order.Total.Amount); // 2*2500 + 1000
        Assert.Equal(1, order.Version);
        Assert.Single(order.StatusHistory);
    }

    [Fact]
    public void Place_UnavailableDish_ThrowsException()
    {
        var dish = CreateValidDish();
        dish.MakeUnavailable();

        var lines = new List<(Dish, int)> { (dish, 1) };

        Assert.Throws<DomainException>(() =>
            Order.Place("user1", "Vevő", "vevo@email.com", "Cím", null, lines, DateTimeOffset.Now));
    }

    [Fact]
    public void Place_DuplicateDishes_MergesToSingleItem()
    {
        var dish = CreateValidDish();
        var lines = new List<(Dish, int)> { (dish, 2), (dish, 3) };

        var order = Order.Place("user1", "Vevő", "vevo@email.com", "Cím", null, lines, DateTimeOffset.Now);

        Assert.Single(order.Items);
        Assert.Equal(5, order.Items.First().Quantity);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsInvalidStatusTransitionException()
    {
        var lines = new List<(Dish, int)> { (CreateValidDish(), 1) };
        var order = Order.Place("u1", "Vevő", "v@v.hu", "Cím", null, lines, DateTimeOffset.Now);

        var ex = Assert.Throws<InvalidStatusTransitionException>(() =>
            order.ChangeStatus(OrderStatus.Delivered, "staff", DateTimeOffset.Now));

        Assert.Equal("Nem engedélyezett állapotváltás: Leadva → Kézbesítve", ex.Message);
    }

    [Fact]
    public void ChangeStatus_ToOutForDeliveryWithoutCourier_ThrowsDomainException()
    {
        var lines = new List<(Dish, int)> { (CreateValidDish(), 1) };
        var order = Order.Place("u1", "V", "v@v", "Cím", null, lines, DateTimeOffset.Now);
        order.ChangeStatus(OrderStatus.Accepted, "staff", DateTimeOffset.Now);
        order.ChangeStatus(OrderStatus.Preparing, "staff", DateTimeOffset.Now);
        order.ChangeStatus(OrderStatus.ReadyForPickup, "staff", DateTimeOffset.Now);

        var ex = Assert.Throws<DomainException>(() =>
            order.ChangeStatus(OrderStatus.OutForDelivery, "staff", DateTimeOffset.Now));

        Assert.Equal("Kiszállítás alatt állapotba csak hozzárendelt futárral léphet a rendelés.", ex.Message);
    }

    [Fact]
    public void AssignCourier_ValidCourier_IncrementsVersion()
    {
        var lines = new List<(Dish, int)> { (CreateValidDish(), 1) };
        var order = Order.Place("u1", "V", "v@v", "Cím", null, lines, DateTimeOffset.Now);
        order.ChangeStatus(OrderStatus.Accepted, "staff", DateTimeOffset.Now);
        var initialVersion = order.Version; // Should be 2 now

        var courier = CreateValidCourier();
        order.AssignCourier(courier);

        Assert.Equal(initialVersion + 1, order.Version);
        Assert.Equal(courier.Id, order.CourierId);
    }

    [Fact(DisplayName = "Kézbesített rendelés nem kerülhet vissza előkészítésbe")]
    [Trait("Category", "CriticalRule")]
    public void Delivered_order_cannot_go_back_to_Preparing()
    {
        var now = DateTimeOffset.Now;
        var lines = new List<(Dish, int)> { (CreateValidDish(), 1) };
        var order = Order.Place("u1", "V", "v@v", "Cím", null, lines, now);
        order.ChangeStatus(OrderStatus.Accepted, "staff", now);
        order.AssignCourier(CreateValidCourier());
        order.ChangeStatus(OrderStatus.Preparing, "staff", now);
        order.ChangeStatus(OrderStatus.ReadyForPickup, "staff", now);
        order.ChangeStatus(OrderStatus.OutForDelivery, "staff", now);
        order.ChangeStatus(OrderStatus.Delivered, "staff", now);

        var ex = Assert.Throws<InvalidStatusTransitionException>(() =>
            order.ChangeStatus(OrderStatus.Preparing, "staff", now));

        Assert.Equal("Nem engedélyezett állapotváltás: Kézbesítve → Készül", ex.Message);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(6, order.StatusHistory.Count);
    }

    [Fact]
    public void GetAllowedNextStatuses_without_courier_does_not_offer_OutForDelivery()
    {
        var now = DateTimeOffset.Now;
        var order = Order.Place("u1", "V", "v@v", "Cím", null, new List<(Dish, int)> { (CreateValidDish(), 1) }, now);
        order.ChangeStatus(OrderStatus.Accepted, "staff", now);
        order.ChangeStatus(OrderStatus.Preparing, "staff", now);
        order.ChangeStatus(OrderStatus.ReadyForPickup, "staff", now);

        Assert.Empty(order.GetAllowedNextStatuses());

        order.AssignCourier(CreateValidCourier());

        Assert.Equal(OrderStatus.OutForDelivery, Assert.Single(order.GetAllowedNextStatuses()));
    }

    [Fact]
    public void GetAllowedNextStatuses_for_new_order_offers_Accepted_and_Cancelled()
    {
        var order = Order.Place("u1", "V", "v@v", "Cím", null, new List<(Dish, int)> { (CreateValidDish(), 1) }, DateTimeOffset.Now);

        var codes = order.GetAllowedNextStatuses().Select(s => s.Code).OrderBy(c => c).ToList();

        Assert.Equal(new List<string> { "Accepted", "Cancelled" }, codes);
    }

    [Fact]
    public void CanAssignCourier_only_in_Accepted_Preparing_or_ReadyForPickup()
    {
        var now = DateTimeOffset.Now;
        var order = Order.Place("u1", "V", "v@v", "Cím", null, new List<(Dish, int)> { (CreateValidDish(), 1) }, now);
        Assert.False(order.CanAssignCourier);

        order.ChangeStatus(OrderStatus.Accepted, "staff", now);
        Assert.True(order.CanAssignCourier);

        order.ChangeStatus(OrderStatus.Cancelled, "staff", now);
        Assert.False(order.CanAssignCourier);
        Assert.Throws<DomainException>(() => order.AssignCourier(CreateValidCourier()));
    }

    [Fact]
    public void AssignCourier_InactiveCourier_ThrowsException()
    {
        var lines = new List<(Dish, int)> { (CreateValidDish(), 1) };
        var order = Order.Place("u1", "V", "v@v", "Cím", null, lines, DateTimeOffset.Now);
        order.ChangeStatus(OrderStatus.Accepted, "staff", DateTimeOffset.Now);

        var courier = CreateValidCourier();
        courier.Deactivate();

        Assert.Throws<DomainException>(() => order.AssignCourier(courier));
    }
}