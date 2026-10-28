using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class CreateCourierRequest
{
    [Required(ErrorMessage = "A futár neve nem lehet üres.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "A futár neve maximum 80 karakter lehet.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A telefonszám nem lehet üres.")]
    [RegularExpression(@"^(\+36|06)( ?\d){9}$",
        ErrorMessage = "A telefonszám formátuma: +36 30 123 4567 vagy 06 30 123 4567.")]
    public string Phone { get; set; } = string.Empty;
}