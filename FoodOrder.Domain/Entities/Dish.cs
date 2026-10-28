using FoodOrder.Domain.Enums;
using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;

namespace FoodOrder.Domain.Entities;

public class Dish
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DishCategory Category { get; private set; }
    public Money Price { get; private set; }
    public bool IsAvailable { get; private set; }

    // EF Core ezzel a konstruktorral hozza létre az objektumot adatbázisból betöltéskor.
#pragma warning disable CS8618
    private Dish() { }
#pragma warning restore CS8618

    private Dish(string name, string description, DishCategory category, Money price)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);

        Name = name.Trim();
        Description = description.Trim();
        Category = category;
        Price = price;
        IsAvailable = true;
    }

    public static Dish Create(string name, string description, DishCategory category, Money price)
    {
        return new Dish(name, description, category, price);
    }

    public void ChangePrice(Money newPrice)
    {
        ValidatePrice(newPrice);
        Price = newPrice;
    }

    public void MakeAvailable() => IsAvailable = true;
    public void MakeUnavailable() => IsAvailable = false;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Az étel neve nem lehet üres.");
        if (name.Trim().Length > 80)
            throw new DomainException("Az étel neve maximum 80 karakter lehet.");
    }

    private static void ValidateDescription(string description)
    {
        if (description?.Length > 300)
            throw new DomainException("A leírás maximum 300 karakter lehet.");
    }

    private static void ValidatePrice(Money price)
    {
        if (price.Amount < 1 || price.Amount > 100000)
            throw new DomainException("Az árnak 1 és 100 000 Ft között kell lennie.");
    }
}