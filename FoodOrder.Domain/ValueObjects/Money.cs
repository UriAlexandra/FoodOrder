using System.Globalization;
using FoodOrder.Domain.Exceptions;

namespace FoodOrder.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; } = "HUF";

    private Money(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Az összeg nem lehet negatív.");
        if (amount % 1 != 0)
            throw new DomainException("Az összeg csak egész szám lehet.");

        Amount = amount;
    }

    public static Money Huf(decimal amount) => new(amount);
    public static Money Zero => new(0);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Különböző pénznemek nem adhatók össze.");
        return new Money(Amount + other.Amount);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("A szorzó nem lehet negatív.");
        return new Money(Amount * quantity);
    }

    public override string ToString()
    {
        var formatted = Amount.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
        return $"{formatted} Ft";
    }
}