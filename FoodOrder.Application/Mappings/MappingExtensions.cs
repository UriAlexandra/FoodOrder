using FoodOrder.Domain.Entities;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared.Models;

namespace FoodOrder.Application.Mappings;

public static class MappingExtensions
{
    public static DishDto ToDto(this Dish dish) => new()
    {
        Id = dish.Id,
        Name = dish.Name,
        Description = dish.Description,
        Category = dish.Category.ToString(),
        Price = dish.Price.Amount,
        IsAvailable = dish.IsAvailable
    };

    public static CourierDto ToDto(this Courier courier) => new()
    {
        Id = courier.Id,
        Name = courier.Name,
        Phone = courier.Phone,
        IsActive = courier.IsActive
    };

    public static OrderDto ToDto(this Order order, Courier? courier = null) => new()
    {
        Id = order.Id,
        PlacedAt = order.PlacedAt,
        CustomerName = order.CustomerName,
        DeliveryAddress = order.DeliveryAddress,
        Note = order.Note,
        Status = order.Status.Code,
        StatusDisplayName = order.Status.DisplayName,
        CourierId = order.CourierId,
        CourierName = courier?.Name,
        CourierPhone = courier?.Phone,
        Total = order.Total.Amount,
        Version = order.Version,
        IsFinal = order.Status.IsFinal,
        CanAssignCourier = order.CanAssignCourier,
        Items = order.Items.Select(i => new OrderItemDto
        {
            DishName = i.DishName,
            Price = i.UnitPrice.Amount,
            Quantity = i.Quantity
        }).ToList(),
        // A választható következő állapotokat a domain adja (futár-feltétellel együtt),
        // így a kliensnek nincs saját átmenettáblája.
        AllowedNextStatuses = order.GetAllowedNextStatuses()
            .Select(s => new StatusOptionDto { Code = s.Code, DisplayName = s.DisplayName })
            .ToList(),
        History = order.StatusHistory.Select(h => new StatusHistoryEntryDto
        {
            Status = h.ToStatus.Code,
            DisplayName = h.ToStatus.DisplayName,
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy
        }).OrderByDescending(h => h.ChangedAt).ToList()
    };
}