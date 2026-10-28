using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Exceptions;
using Xunit;

namespace FoodOrder.Domain.Tests.Entities;

public class CourierTests
{
    [Theory]
    [InlineData("+36 30 123 4567")]
    [InlineData("06 20 1234567")]
    [InlineData("+36701234567")]
    public void Create_ValidPhone_CreatesCourier(string phone)
    {
        var courier = Courier.Create("Kiss Futár", phone);
        Assert.NotNull(courier);
    }

    [Theory]
    [InlineData("+36 30 123 456")] // Rövid
    [InlineData("+36 30 123 45678")] // Hosszú
    [InlineData("07 20 1234567")] // Rossz előtag
    [InlineData("+36 30 12A 4567")] // Betű
    public void Create_InvalidPhone_ThrowsException(string phone)
    {
        Assert.Throws<DomainException>(() => Courier.Create("Futár", phone));
    }
}