using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

/// <summary>
/// Új rendelés. A vásárló adatait (azonosító, név, e-mail) a szerver a bejelentkezett felhasználóból veszi.
/// </summary>
public class PlaceOrderRequest
{
    [Required(ErrorMessage = "A szállítási cím megadása kötelező!")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "A cím legalább 5, legfeljebb 200 karakter lehet!")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "A megjegyzés maximum 300 karakter lehet.")]
    public string? Note { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "A rendelésnek legalább egy tételt kell tartalmaznia.")]
    public List<PlaceOrderItemRequest> Items { get; set; } = new();
}
