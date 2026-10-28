namespace FoodOrder.Shared;

/// <summary>
/// Az ételkategóriák kódja (a domain DishCategory értékei) és magyar megjelenítési neve.
/// </summary>
public static class DishCategories
{
    public static readonly IReadOnlyList<(string Code, string Label)> All =
    [
        ("Soup", "Leves"),
        ("MainCourse", "Főétel"),
        ("Dessert", "Desszert"),
        ("Drink", "Ital")
    ];

    public static string Label(string code)
    {
        foreach (var (c, label) in All)
        {
            if (c == code) return label;
        }
        return code;
    }
}
