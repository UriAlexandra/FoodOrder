using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "A név megadása kötelező.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A név 2 és 100 karakter között lehet.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Az e-mail-cím megadása kötelező.")]
    [EmailAddress(ErrorMessage = "Érvénytelen e-mail-cím.")]
    [StringLength(256, ErrorMessage = "Az e-mail-cím legfeljebb 256 karakter lehet.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A jelszó megadása kötelező.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A jelszónak legalább 8 karakter hosszúnak kell lennie.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Add meg újra a jelszót.")]
    [Compare(nameof(Password), ErrorMessage = "A két jelszó nem egyezik.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
