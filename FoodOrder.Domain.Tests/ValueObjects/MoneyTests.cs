using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;
using Xunit;

namespace FoodOrder.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Huf_NegativeAmount_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Money.Huf(-100));
    }

    [Fact]
    public void Huf_FractionalAmount_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Money.Huf(10.5m));
    }

    [Fact]
    public void Add_ValidAmounts_ReturnsSum()
    {
        var m1 = Money.Huf(1000);
        var m2 = Money.Huf(1490);

        var result = m1.Add(m2);

        Assert.Equal(2490, result.Amount);
    }

    [Fact]
    public void Multiply_ValidQuantity_ReturnsProduct()
    {
        var money = Money.Huf(2490);
        var result = money.Multiply(3);

        Assert.Equal(7470, result.Amount);
    }

    [Fact]
    public void Multiply_NegativeQuantity_ThrowsException()
    {
        var money = Money.Huf(100);
        Assert.Throws<DomainException>(() => money.Multiply(-1));
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var money = Money.Huf(12490);
        Assert.Equal("12 490 Ft", money.ToString());
    }
}