namespace FoodOrder.Api;

public static class AuthPolicies
{
    public const string StaffOnly = nameof(StaffOnly);
    public const string CustomerOnly = nameof(CustomerOnly);
}
