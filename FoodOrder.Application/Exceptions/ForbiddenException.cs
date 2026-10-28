namespace FoodOrder.Application.Exceptions;

/// <summary>A felhasználó be van jelentkezve, de a műveletre nincs jogosultsága (403).</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
