using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Infrastructure.Email;

/// <summary>
/// SMTP-beállítások az appsettings „Smtp” szekciójából. Indításkor ellenőrizzük (ValidateOnStart).
/// Jelszót csak user-secretsben vagy környezeti változóban tárolj, appsettings*.json-ban soha.
/// </summary>
public class SmtpOptions : IValidatableObject
{
    public const string SectionName = "Smtp";

    [Required(ErrorMessage = "Az Smtp:Host megadása kötelező.")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Az Smtp:Port 1 és 65535 közötti szám legyen.")]
    public int Port { get; set; } = 587;

    /// <summary>STARTTLS titkosítás. Az smtp4dev-hez kikapcsolva, valódi szolgáltatónál bekapcsolva.</summary>
    public bool UseStartTls { get; set; } = true;

    public string? UserName { get; set; }
    public string? Password { get; set; }

    [Required(ErrorMessage = "Az Smtp:FromAddress megadása kötelező.")]
    [EmailAddress(ErrorMessage = "Az Smtp:FromAddress érvényes e-mail-cím legyen.")]
    public string FromAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Az Smtp:FromName megadása kötelező.")]
    public string FromName { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(UserName) && string.IsNullOrEmpty(Password))
            yield return new ValidationResult(
                "Az Smtp:UserName mellé az Smtp:Password is kell (user-secretsben).", [nameof(Password)]);
    }
}
