using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;

namespace FoodOrder.Domain.Entities;

public class OrderItem
{
    public int Id { get; private set; }
    public int DishId { get; private set; }
    public string DishName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    // EF Core ezzel a konstruktorral hozza létre az objektumot adatbázisból betöltéskor.
#pragma warning disable CS8618
    private OrderItem() { }
#pragma warning restore CS8618

    internal OrderItem(int dishId, string dishName, Money unitPrice, int quantity)
    {
        if (quantity < 1 || quantity > 20)
            throw new DomainException("A mennyiség 1 és 20 között lehet.");

        DishId = dishId;
        DishName = dishName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    internal void AddQuantity(int additionalQuantity)
    {
        var newQuantity = Quantity + additionalQuantity;
        if (newQuantity > 20)
            throw new DomainException("Egy ételből összesen maximum 20 darab rendelhető.");
        Quantity = newQuantity;
    }
}