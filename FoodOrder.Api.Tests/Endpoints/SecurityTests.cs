using System.Net;
using System.Net.Http.Json;
using FoodOrder.Api.Tests.Infrastructure;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FoodOrder.Api.Tests.Endpoints;

/// <summary>A biztonsági átnézés eredményei, hogy egy későbbi módosítás ne rontsa el őket.</summary>
public class SecurityTests(FoodOrderApiFactory factory) : IClassFixture<FoodOrderApiFactory>
{
    /// <summary>Szándékosan névtelenül elérhető végpontok. Minden más API- és hub-végponthoz bejelentkezés kell.</summary>
    private static readonly string[] PublicEndpoints =
    {
        "GET /api/auth/me",
        "GET /api/dishes",
        "POST /api/auth/login",
        "POST /api/auth/logout",
        "POST /api/auth/register"
    };

    [Fact]
    public void EveryApiEndpoint_RequiresLogin_ExceptThePublicOnes()
    {
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText is { } path && (path.StartsWith("/api", StringComparison.Ordinal) || path.StartsWith("/hubs", StringComparison.Ordinal)))
            .ToList();

        var withoutLogin = endpoints
            .Where(e => e.Metadata.GetMetadata<IAuthorizeData>() is null || e.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Select(Describe)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(endpoints.Count > PublicEndpoints.Length, "Nem találhatók az API-végpontok.");
        Assert.Equal(PublicEndpoints, withoutLogin);
    }

    [Fact]
    public async Task LockedOutAccount_GetsTheSameGenericMessage_AsAWrongPassword()
    {
        const string email = "zarolas@teszt.hu";
        const string password = "Jelszo123";
        var registration = await (await factory.CreateClientAsync()).PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { FullName = "Zárolás Teszt", Email = email, Password = password, ConfirmPassword = password });
        Assert.Equal(HttpStatusCode.OK, registration.StatusCode);

        var client = await factory.CreateClientAsync();
        for (var attempt = 1; attempt <= 5; attempt++)
            await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = "Rossz1234" });

        // A zárolt fiók a helyes jelszóval sem enged be, és az üzenet nem árulja el a zárolást.
        var locked = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        var unknown = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "nincsilyen@teszt.hu", Password = password });

        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal("Hibás e-mail-cím vagy jelszó.", (await locked.ReadProblemAsync()).Detail);
        Assert.Equal("Hibás e-mail-cím vagy jelszó.", (await unknown.ReadProblemAsync()).Detail);
    }

    [Fact]
    public async Task Order_WithTooManyItems_IsRejectedBeforeTouchingTheDatabase()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var dishes = await anna.GetFromJsonAsync<List<DishDto>>("/api/dishes?onlyAvailable=true");
        var request = new PlaceOrderRequest
        {
            DeliveryAddress = "1055 Budapest, Kossuth Lajos tér 1.",
            Items = Enumerable.Range(0, 51).Select(_ => new PlaceOrderItemRequest { DishId = dishes![0].Id, Quantity = 1 }).ToList()
        };

        var response = await anna.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(new[] { "Egy rendelésben legfeljebb 50 tétel lehet." }, problem!.Errors["Items"]);
    }

    [Theory]
    [InlineData("/api/dishes")]  // sikeres válasz
    [InlineData("/api/orders")]  // 401-es hibaválasz
    public async Task Responses_CarrySecurityHeaders(string path)
    {
        var client = await factory.CreateClientAsync();

        var response = await client.GetAsync(path);

        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(response.Headers.GetValues("X-Frame-Options")));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
    }

    private static string Describe(RouteEndpoint endpoint)
    {
        var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? new[] { "*" };
        return $"{string.Join(",", methods)} {endpoint.RoutePattern.RawText!.TrimEnd('/')}";
    }
}
