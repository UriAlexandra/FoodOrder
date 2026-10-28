namespace FoodOrder.Client.Storage;

public static class StorageKeys
{
    /// <summary>Kosár felhasználónként, hogy egy közös gépen ne keveredjenek a kosarak.</summary>
    public static string CartFor(string email) => $"foodorder.cart.{email.Trim().ToLowerInvariant()}";

    // Korábbi hetek kulcsai: induláskor töröljük őket.
    public static readonly string[] Legacy =
    [
        "foodorder.dishes",   // 1–3. hét: étlap a böngészőben
        "foodorder.orders",   // 1–3. hét: rendelések a böngészőben
        "foodorder.role",     // 1–4. hét: demó szerepkörváltó
        "foodorder.cart"      // 1–4. hét: közös kosár
    ];
}
