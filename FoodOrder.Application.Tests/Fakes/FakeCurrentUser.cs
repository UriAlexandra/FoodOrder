using FoodOrder.Application.Ports;

namespace FoodOrder.Application.Tests.Fakes;

/// <summary>A bejelentkezett felhasználó a tesztekben.</summary>
public sealed record FakeCurrentUser(string? UserId, string? Email, string? FullName, bool IsStaff, bool IsCustomer) : ICurrentUser
{
    public bool IsAuthenticated => UserId is not null;

    public static FakeCurrentUser Anna { get; } = new("anna-id", "anna@foodorder.hu", "Kiss Anna", IsStaff: false, IsCustomer: true);
    public static FakeCurrentUser Bela { get; } = new("bela-id", "bela@foodorder.hu", "Kovács Béla", IsStaff: false, IsCustomer: true);
    public static FakeCurrentUser Staff { get; } = new("staff-id", "dolgozo@foodorder.hu", "Nagy Péter", IsStaff: true, IsCustomer: false);
}
