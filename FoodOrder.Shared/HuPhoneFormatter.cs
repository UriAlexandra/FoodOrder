namespace FoodOrder.Shared;

/// <summary>
/// Megjelenítéshez tagolja a szóközök nélkül tárolt magyar mobilszámot: +36201234567 → +36 20 123 4567.
/// </summary>
public static class HuPhoneFormatter
{
    public static string Format(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

        var clean = phone.Replace(" ", "");
        var (prefix, digits) = clean.StartsWith("+36") ? ("+36", clean[3..])
            : clean.StartsWith("06") ? ("06", clean[2..])
            : ("", clean);

        return digits.Length == 9 && prefix.Length > 0
            ? $"{prefix} {digits[..2]} {digits[2..5]} {digits[5..]}"
            : phone;
    }
}
