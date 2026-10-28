using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;
using Xunit;

namespace FoodOrder.Domain.Tests.ValueObjects;

public class OrderStatusTests
{
    // A feladatleírás szerinti megengedett átmenetek. Minden más pár tilos.
    private static readonly HashSet<(string From, string To)> AllowedTransitions =
    [
        ("Placed", "Accepted"),
        ("Placed", "Cancelled"),
        ("Accepted", "Preparing"),
        ("Accepted", "Cancelled"),
        ("Preparing", "ReadyForPickup"),
        ("ReadyForPickup", "OutForDelivery"),
        ("OutForDelivery", "Delivered")
    ];

    public static IEnumerable<object[]> AllStatusPairs()
    {
        foreach (var from in OrderStatus.All)
        {
            foreach (var to in OrderStatus.All)
            {
                yield return new object[] { from, to };
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllStatusPairs))]
    public void CanTransitionTo_matches_the_transition_table(OrderStatus from, OrderStatus to)
    {
        var expected = AllowedTransitions.Contains((from.Code, to.Code));

        Assert.Equal(expected, from.CanTransitionTo(to));
    }

    [Fact]
    public void Delivered_cannot_go_back_to_Preparing()
    {
        Assert.False(OrderStatus.Delivered.CanTransitionTo(OrderStatus.Preparing));
    }

    [Fact]
    public void Delivered_and_Cancelled_are_final_states()
    {
        Assert.True(OrderStatus.Delivered.IsFinal);
        Assert.True(OrderStatus.Cancelled.IsFinal);
        Assert.Empty(OrderStatus.Delivered.AllowedNext);
        Assert.Empty(OrderStatus.Cancelled.AllowedNext);
    }

    [Fact]
    public void FromCode_returns_the_matching_status()
    {
        Assert.Equal(OrderStatus.OutForDelivery, OrderStatus.FromCode("OutForDelivery"));
    }

    [Fact]
    public void FromCode_with_unknown_code_throws_DomainException()
    {
        Assert.Throws<DomainException>(() => OrderStatus.FromCode("InvalidCode123"));
    }
}
