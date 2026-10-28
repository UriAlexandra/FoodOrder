using Microsoft.AspNetCore.Identity;

namespace FoodOrder.Infrastructure.Identity;

/// <summary>Az ASP.NET Core Identity hibaüzenetei magyarul.</summary>
public class HungarianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Error(nameof(DefaultError), "Ismeretlen hiba történt.");

    public override IdentityError ConcurrencyFailure() =>
        Error(nameof(ConcurrencyFailure), "Az adatot közben módosították, próbáld újra.");

    public override IdentityError PasswordMismatch() =>
        Error(nameof(PasswordMismatch), "Hibás jelszó.");

    public override IdentityError InvalidUserName(string? userName) =>
        Error(nameof(InvalidUserName), "Érvénytelen e-mail-cím.");

    public override IdentityError InvalidEmail(string? email) =>
        Error(nameof(InvalidEmail), "Érvénytelen e-mail-cím.");

    public override IdentityError DuplicateUserName(string userName) =>
        Error(nameof(DuplicateUserName), "Ezzel az e-mail-címmel már regisztráltak.");

    public override IdentityError DuplicateEmail(string email) =>
        Error(nameof(DuplicateEmail), "Ezzel az e-mail-címmel már regisztráltak.");

    public override IdentityError PasswordTooShort(int length) =>
        Error(nameof(PasswordTooShort), $"A jelszónak legalább {length} karakter hosszúnak kell lennie.");

    public override IdentityError PasswordRequiresDigit() =>
        Error(nameof(PasswordRequiresDigit), "A jelszónak tartalmaznia kell számjegyet.");

    public override IdentityError PasswordRequiresLower() =>
        Error(nameof(PasswordRequiresLower), "A jelszónak tartalmaznia kell kisbetűt.");

    public override IdentityError PasswordRequiresUpper() =>
        Error(nameof(PasswordRequiresUpper), "A jelszónak tartalmaznia kell nagybetűt.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric), "A jelszónak tartalmaznia kell speciális karaktert.");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Error(nameof(PasswordRequiresUniqueChars), $"A jelszónak legalább {uniqueChars} különböző karaktert kell tartalmaznia.");

    public override IdentityError UserAlreadyInRole(string role) =>
        Error(nameof(UserAlreadyInRole), "A felhasználó már rendelkezik ezzel a szerepkörrel.");

    private static IdentityError Error(string code, string description) =>
        new() { Code = code, Description = description };
}
