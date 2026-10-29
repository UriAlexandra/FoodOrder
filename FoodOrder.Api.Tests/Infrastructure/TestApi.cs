using System.Net;
using System.Net.Http.Json;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Api.Tests.Infrastructure;

/// <summary>A három tesztfiók (a seeder hozza létre).</summary>
public static class TestAccounts
{
    public const string Staff = "dolgozo@foodorder.hu";
    public const string Anna = "anna@foodorder.hu";
    public const string Bela = "bela@foodorder.hu";
    public const string Password = IdentitySeeder.DemoPassword;
}

/// <summary>Rövid segédmetódusok, hogy a tesztek a szándékot mutassák, ne a HTTP-részleteket.</summary>
public static class TestApi
{
    public static async Task LoginAsync(this HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = TestAccounts.Password });
        response.EnsureSuccessStatusCode();
    }

    public static async Task<int> PlaceOrderAsync(this HttpClient client, int quantity = 1)
    {
        var dishes = await client.GetFromJsonAsync<List<DishDto>>("/api/dishes?onlyAvailable=true");
        var request = new PlaceOrderRequest
        {
            DeliveryAddress = "1055 Budapest, Kossuth Lajos tér 1.",
            Items = [new PlaceOrderItemRequest { DishId = dishes![0].Id, Quantity = quantity }]
        };

        var response = await client.PostAsJsonAsync("/api/orders", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PlaceOrderResponse>())!.OrderId;
    }

    public static async Task<OrderDto> GetOrderAsync(this HttpClient client, int orderId) =>
        (await client.GetFromJsonAsync<OrderDto>($"/api/orders/{orderId}"))!;

    public static Task<HttpResponseMessage> ChangeStatusAsync(this HttpClient client, int orderId, string newStatus, int expectedVersion) =>
        client.PostAsJsonAsync($"/api/orders/{orderId}/status",
            new ChangeOrderStatusRequest { NewStatus = newStatus, ExpectedVersion = expectedVersion });

    /// <summary>Állapotváltás, amelynek sikerülnie kell; a frissített rendelést adja vissza.</summary>
    public static async Task<OrderDto> ChangeStatusOkAsync(this HttpClient client, int orderId, string newStatus, int expectedVersion)
    {
        var response = await client.ChangeStatusAsync(orderId, newStatus, expectedVersion);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    public static async Task<OrderDto> AssignCourierAsync(this HttpClient client, int orderId, int courierId, int expectedVersion)
    {
        var response = await client.PostAsJsonAsync($"/api/orders/{orderId}/courier",
            new AssignCourierRequest { CourierId = courierId, ExpectedVersion = expectedVersion });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    public static async Task<ProblemDetails> ReadProblemAsync(this HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
    }
}
