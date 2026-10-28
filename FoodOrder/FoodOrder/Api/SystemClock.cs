using FoodOrder.Application.Ports;

namespace FoodOrder.Api;

public class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}