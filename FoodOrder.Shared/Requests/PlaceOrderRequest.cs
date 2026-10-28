using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class PlaceOrderRequest
{
    [Required(ErrorMessage = "A szállítási cím megadása kötelező!")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "A cím legalább 5, legfeljebb 200 karakter lehet!")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "A megjegyzés maximum 300 karakter lehet.")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Vásárló nevének megadása kötelező.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vásárló email címének megadása kötelező.")]
    [EmailAddress(ErrorMessage = "Érvénytelen email formátum.")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "A rendelésnek legalább egy tételt kell tartalmaznia.")]
    public List<PlaceOrderItemRequest> Items { get; set; } = new();
}