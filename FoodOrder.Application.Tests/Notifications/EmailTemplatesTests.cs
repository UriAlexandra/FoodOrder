using FoodOrder.Application.Notifications;
using FoodOrder.Application.Tests.Fakes;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Domain.ValueObjects;

namespace FoodOrder.Application.Tests.Notifications;

/// <summary>A levélbe kerülő, felhasználótól származó szöveg a HTML-részben kódolva, a szöveges részben változatlanul jelenik meg.</summary>
public class EmailTemplatesTests
{
    private static Order OrderWithMarkup()
    {
        var dish = Dish.Create("Rántott <i>karaj</i> & püré", "Házi", DishCategory.MainCourse, Money.Huf(3490));
        var order = Order.Place("anna-id", "Kiss <Anna>", "anna@foodorder.hu", "Fő utca 1. <script>alert(1)</script>",
            "<b>teszt</b>", new List<(Dish, int)> { (dish, 1) }, new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.FromHours(1)));
        EntityIds.Assign(order, 42);
        return order;
    }

    [Fact]
    public void UserProvidedText_IsEncodedInHtml_AndKeptAsIsInText()
    {
        var mail = EmailTemplates.Render(OrderWithMarkup(), courier: null, OrderEventType.Placed, OrderStatus.Placed);

        Assert.Contains("Megjegyzés: &lt;b&gt;teszt&lt;/b&gt;", mail.HtmlBody);
        Assert.Contains("Kedves Kiss &lt;Anna&gt;!", mail.HtmlBody);
        Assert.Contains("Fő utca 1. &lt;script&gt;alert(1)&lt;/script&gt;", mail.HtmlBody);
        Assert.Contains("Rántott &lt;i&gt;karaj&lt;/i&gt; &amp; püré", mail.HtmlBody);
        Assert.DoesNotContain("<b>teszt", mail.HtmlBody);
        Assert.DoesNotContain("<script>", mail.HtmlBody);
        Assert.DoesNotContain("<i>karaj", mail.HtmlBody);

        Assert.Contains("<b>teszt</b>", mail.TextBody);
        Assert.Equal("FoodOrder #42 – Rendelés beérkezett", mail.Subject);
        Assert.Equal("anna@foodorder.hu", mail.To);
    }

    [Fact]
    public void CourierName_IsEncodedInHtml()
    {
        var order = OrderWithMarkup();
        var courier = Courier.Create("Vincze <u>Gábor</u>", "+36 20 123 4567");

        var mail = EmailTemplates.Render(order, courier, OrderEventType.StatusChanged, OrderStatus.OutForDelivery);

        Assert.Contains("Futár: Vincze &lt;u&gt;Gábor&lt;/u&gt;", mail.HtmlBody);
        Assert.DoesNotContain("<u>", mail.HtmlBody);
        Assert.Equal("FoodOrder #42 – Kiszállítás alatt", mail.Subject);
    }
}
