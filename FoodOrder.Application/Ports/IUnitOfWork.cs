namespace FoodOrder.Application.Ports;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}