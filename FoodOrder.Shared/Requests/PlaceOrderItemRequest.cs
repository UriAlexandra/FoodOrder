using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class PlaceOrderItemRequest
{
    public int DishId { get; set; }

    [Range(1, 20, ErrorMessage = "A mennyiség 1 és 20 között lehet.")]
    public int Quantity { get; set; }
}