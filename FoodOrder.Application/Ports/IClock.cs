namespace FoodOrder.Application.Ports;

public interface IClock
{
    DateTimeOffset Now { get; }
}