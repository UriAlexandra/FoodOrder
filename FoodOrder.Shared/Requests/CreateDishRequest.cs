using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class CreateDishRequest
{
    [Required(ErrorMessage = "Az étel neve nem lehet üres.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Az étel neve maximum 80 karakter lehet.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "A leírás maximum 300 karakter lehet.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategória megadása kötelező.")]
    public string Category { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "Az árnak 1 és 100 000 Ft között kell lennie.")]
    public decimal Price { get; set; }
}