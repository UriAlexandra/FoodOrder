using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;
using Xunit;

namespace FoodOrder.Domain.Tests.Entities;

public class DishTests
{
    [Fact]
    public void Create_ValidData_CreatesDish()
    {
        var dish = Dish.Create("Gulyásleves", "Finom gulyás", DishCategory.Soup, Money.Huf(2000));

        Assert.Equal("Gulyásleves", dish.Name);
        Assert.True(dish.IsAvailable);
        Assert.Equal(2000, dish.Price.Amount);
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        Assert.Throws<DomainException>(() => Dish.Create("   ", "Desc", DishCategory.Soup, Money.Huf(1000)));
    }

    [Fact]
    public void Create_PriceTooHigh_ThrowsException()
    {
        Assert.Throws<DomainException>(() => Dish.Create("Arany leves", "Desc", DishCategory.Soup, Money.Huf(100001)));
    }
}