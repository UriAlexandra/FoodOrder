using System.Net;
using System.Net.Http.Json;
using FoodOrder.Api.Tests.Infrastructure;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Api.Tests.Endpoints;

/// <summary>Ki mit érhet el: a jogosultságot a szerver dönti el, a felülettől függetlenül.</summary>
public class AuthorizationTests(FoodOrderApiFactory factory) : IClassFixture<FoodOrderApiFactory>
{
    private static readonly CreateDishRequest NewDish = new()
    {
        Name = "Túrós csusza",
        Description = "Pirított szalonnával",
        Category = "MainCourse",
        Price = 2890
    };

    [Fact]
    public async Task CreateDish_Anonymous_Returns401_WithoutRedirect()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/dishes", NewDish);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Equal("A folytatáshoz jelentkezz be.", (await response.ReadProblemAsync()).Detail);
    }

    [Fact]
    public async Task CreateDish_AsCustomer_Returns403()
    {
        var client = await factory.CreateClientAsync(TestAccounts.Anna);

        var response = await client.PostAsJsonAsync("/api/dishes", NewDish);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task CreateDish_AsStaff_Returns201()
    {
        var client = await factory.CreateClientAsync(TestAccounts.Staff);

        var response = await client.PostAsJsonAsync("/api/dishes", NewDish);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dish = await response.Content.ReadFromJsonAsync<DishDto>();
        Assert.Equal("Túrós csusza", dish!.Name);
        Assert.Equal($"/api/dishes/{dish.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Menu_IsVisibleWithoutLogin()
    {
        var client = await factory.CreateClientAsync();

        var dishes = await client.GetFromJsonAsync<List<DishDto>>("/api/dishes");

        Assert.NotEmpty(dishes!);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401_WithGenericMessage()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = TestAccounts.Bela, Password = "Rossz1234" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Hibás e-mail-cím vagy jelszó.", (await response.ReadProblemAsync()).Detail);
    }

    [Fact]
    public async Task Login_SetsSecureHttpOnlyStrictCookie()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = TestAccounts.Anna, Password = TestAccounts.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith("FoodOrder.Auth=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
    }
}
