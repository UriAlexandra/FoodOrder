namespace FoodOrder.Client.Storage;

public static class StorageKeys
{
    public const string Role = "foodorder.role";
    public const string Cart = "foodorder.cart";

    // Az 1–3. hét localStorage-kulcsai: az étlap és a rendelések már a szerveren vannak,
    // induláskor töröljük őket.
    public const string LegacyDishes = "foodorder.dishes";
    public const string LegacyOrders = "foodorder.orders";
}
