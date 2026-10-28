namespace FoodOrder.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string message) : base(message) { }
}