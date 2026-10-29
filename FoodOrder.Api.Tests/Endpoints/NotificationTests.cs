using FoodOrder.Api.Tests.Infrastructure;

namespace FoodOrder.Api.Tests.Endpoints;

/// <summary>E-mail-értesítés az outboxon keresztül (a küldő fake, a feldolgozó kört a teszt indítja).</summary>
public class NotificationTests(FoodOrderApiFactory factory) : IClassFixture<FoodOrderApiFactory>
{
    [Fact]
    public async Task PlacingAnOrder_AndOneOutboxRound_SendsExactlyOneEmailToAnna()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var id = await anna.PlaceOrderAsync();

        await factory.RunOutboxOnceAsync();

        var email = Assert.Single(factory.Emails.Sent, m => m.Subject.StartsWith($"FoodOrder #{id} ", StringComparison.Ordinal));
        Assert.Equal(TestAccounts.Anna, email.To);
        Assert.Equal("Kiss Anna", email.ToName);
        Assert.Equal($"FoodOrder #{id} – Rendelés beérkezett", email.Subject);
        Assert.Contains("Köszönjük a rendelésed!", email.TextBody);
    }

    [Fact]
    public async Task ProcessedMessage_IsNeverSentAgain_AndStatusChangeSendsOneMore()
    {
        var anna = await factory.CreateClientAsync(TestAccounts.Anna);
        var staff = await factory.CreateClientAsync(TestAccounts.Staff);
        var id = await anna.PlaceOrderAsync();
        await factory.RunOutboxOnceAsync();

        await staff.ChangeStatusOkAsync(id, "Accepted", 1);
        await factory.RunOutboxOnceAsync();
        await factory.RunOutboxOnceAsync();

        var subjects = factory.Emails.Sent
            .Where(m => m.Subject.StartsWith($"FoodOrder #{id} ", StringComparison.Ordinal))
            .Select(m => m.Subject);
        Assert.Equal(new[] { $"FoodOrder #{id} – Rendelés beérkezett", $"FoodOrder #{id} – Elfogadva" }, subjects);
    }
}
