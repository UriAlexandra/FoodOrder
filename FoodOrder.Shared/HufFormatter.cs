using System.Globalization;

namespace FoodOrder.Shared;

public static class HufFormatter
{
    public static string Format(decimal value)
    {
        long longVal = (long)Math.Round(value);
        string str = longVal.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
        return $"{str} Ft";
    }
}