using FoodOrder.Domain.Exceptions;

namespace FoodOrder.Domain.Entities;

public class Courier
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core ezzel a konstruktorral hozza létre az objektumot adatbázisból betöltéskor.
#pragma warning disable CS8618
    private Courier() { }
#pragma warning restore CS8618

    private Courier(string name, string phone)
    {
        ValidateName(name);
        ValidatePhone(phone);

        Name = name.Trim();
        Phone = phone.Replace(" ", "");
        IsActive = true;
    }

    public static Courier Create(string name, string phone) => new Courier(name, phone);

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("A futár neve nem lehet üres.");
        if (name.Trim().Length > 80)
            throw new DomainException("A futár neve maximum 80 karakter lehet.");
    }

    private static void ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("A telefonszám nem lehet üres.");

        var clean = phone.Replace(" ", "");
        bool isValidPrefix = (clean.StartsWith("+36") && clean.Length == 12) ||
                             (clean.StartsWith("06") && clean.Length == 11);

        if (!isValidPrefix)
            throw new DomainException("A telefonszám formátuma érvénytelen (+36 vagy 06 előtag, 9 számjegy szükséges).");

        var digitsPart = clean.StartsWith("+36") ? clean.Substring(3) : clean.Substring(2);
        if (!digitsPart.All(char.IsDigit))
            throw new DomainException("A telefonszám csak számjegyeket tartalmazhat az előtag után.");
    }
}