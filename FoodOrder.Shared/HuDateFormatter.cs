using System.Globalization;

namespace FoodOrder.Shared;

/// <summary>
/// Magyar dátumformátum (pl. 2026. 10. 25. 18:05) helyi időben, a böngésző nyelvi beállításától függetlenül.
/// </summary>
public static class HuDateFormatter
{
    public static string Format(DateTimeOffset value) =>
        value.ToLocalTime().ToString("yyyy. MM. dd. HH:mm", CultureInfo.InvariantCulture);
}
