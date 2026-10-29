namespace FoodOrder.Shared.Realtime;

/// <summary>A valós idejű rendelésértesítések közös nevei – a szerver és a kliens ugyanezt használja.</summary>
public static class OrderHubContract
{
    /// <summary>A hub címe (a kliens az alkalmazás alapcíméhez képest éri el).</summary>
    public const string Path = "/hubs/orders";

    /// <summary>A szerver ezzel a névvel küldi a változásról szóló üzenetet.</summary>
    public const string OrderChanged = "OrderChanged";
}
