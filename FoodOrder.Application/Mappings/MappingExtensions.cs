using FoodOrder.Application.Policies;
using FoodOrder.Application.Ports;
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

    /// <summary>
    /// A rendelés a <paramref name="user"/> szemszögéből: a választható állapotokat és a futár-hozzárendelést
    /// a szerepköre szerint kapja meg.
    /// </summary>
    public static OrderDto ToDto(this Order order, Courier? courier, ICurrentUser user) => new()
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
        CanAssignCourier = user.IsStaff && order.CanAssignCourier,
        Items = order.Items.Select(i => new OrderItemDto
        {
            DishName = i.DishName,
            Price = i.UnitPrice.Amount,
            Quantity = i.Quantity
        }).ToList(),
        // A választható következő állapotokat a domain és a szerepkör-szabály együtt adja,
        // így a kliensnek nincs saját átmenettáblája.
        AllowedNextStatuses = OrderStatusPolicy.AllowedNextFor(order, user)
            .Select(s => new StatusOptionDto { Code = s.Code, DisplayName = s.DisplayName })
            .ToList(),
        // Legújabb elöl; azonos időpontnál a később rögzített bejegyzés kerül előre.
        History = order.StatusHistory
            .OrderByDescending(h => h.ChangedAt)
            .ThenByDescending(h => h.Id)
            .Select(h => new StatusHistoryEntryDto
            {
                Status = h.ToStatus.Code,
                DisplayName = h.ToStatus.DisplayName,
                ChangedAt = h.ChangedAt,
                ChangedBy = h.ChangedBy
            })
            .ToList()
    };
}