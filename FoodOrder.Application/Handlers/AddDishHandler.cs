using FoodOrder.Application.Ports;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Domain.Exceptions;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Application.Handlers;

public class AddDishHandler(IDishRepository repository, IUnitOfWork uow)
{
    public async Task HandleAsync(CreateDishRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<DishCategory>(request.Category, ignoreCase: true, out var category)
            || !Enum.IsDefined(category))
        {
            throw new DomainException($"Ismeretlen kategória: {request.Category}");
        }

        var dish = Dish.Create(request.Name, request.Description, category, Money.Huf(request.Price));
        await repository.AddAsync(dish, ct);
        await uow.SaveChangesAsync(ct);
    }
}
