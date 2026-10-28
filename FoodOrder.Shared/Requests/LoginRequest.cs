using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Az e-mail-cím megadása kötelező.")]
    [EmailAddress(ErrorMessage = "Érvénytelen e-mail-cím.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A jelszó megadása kötelező.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
