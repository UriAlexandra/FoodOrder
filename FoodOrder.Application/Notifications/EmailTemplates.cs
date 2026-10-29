using System.Text;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.ValueObjects;
using FoodOrder.Shared;

namespace FoodOrder.Application.Notifications;

/// <summary>
/// A vásárlónak küldött levelek magyar sablonjai, HTML és sima szöveges változatban.
/// A felhasználótól származó adatokat (név, cím, megjegyzés, ételnév, futár) a HTML-ben mindig kódoljuk,
/// így egy megjegyzésbe írt &lt;b&gt;teszt&lt;/b&gt; a levélben szövegként jelenik meg.
/// </summary>
public static class EmailTemplates
{
    private const string Footer = "Ez egy automatikus értesítés a FoodOrder rendszerből, kérjük, ne válaszolj rá.";

    /// <param name="order">A rendelés a küldés pillanatában (tételek, cím, vásárló).</param>
    /// <param name="courier">A hozzárendelt futár, ha van.</param>
    /// <param name="type">Leadás vagy állapotváltás.</param>
    /// <param name="status">Az esemény idején érvényes állapot (az outbox rögzíti), nem feltétlenül a mostani.</param>
    public static EmailMessage Render(Order order, Courier? courier, OrderEventType type, OrderStatus status)
    {
        var content = Describe(order.Id, type, status, courier);
        var subject = $"FoodOrder #{order.Id} – {content.Title}";

        return new EmailMessage(
            To: order.CustomerEmail,
            ToName: order.CustomerName,
            Subject: subject,
            HtmlBody: BuildHtml(order, subject, content),
            TextBody: BuildText(order, content));
    }

    private sealed record Content(string Title, string Lead, string? CourierLine);

    private static Content Describe(int orderId, OrderEventType type, OrderStatus status, Courier? courier)
    {
        if (type == OrderEventType.Placed)
            return new("Rendelés beérkezett",
                $"Köszönjük a rendelésed! A(z) #{orderId} rendelést megkaptuk, az étterem hamarosan visszaigazolja.", null);

        if (status == OrderStatus.Accepted)
            return new(status.DisplayName, $"Az étterem elfogadta a(z) #{orderId} rendelésedet.", null);
        if (status == OrderStatus.Preparing)
            return new(status.DisplayName, $"A(z) #{orderId} rendelésed már készül a konyhán.", null);
        if (status == OrderStatus.ReadyForPickup)
            return new(status.DisplayName, $"A(z) #{orderId} rendelésed elkészült, hamarosan átadjuk a futárnak.", null);
        if (status == OrderStatus.OutForDelivery)
            return new(status.DisplayName, $"A(z) #{orderId} rendelésed úton van hozzád.",
                courier is null
                    ? "A futár adatait a FoodOrder oldalán, a rendelés követésénél látod."
                    : $"Futár: {courier.Name}, telefon: {HuPhoneFormatter.Format(courier.Phone)}");
        if (status == OrderStatus.Delivered)
            return new(status.DisplayName, $"A(z) #{orderId} rendelésedet kézbesítettük. Jó étvágyat!", null);
        if (status == OrderStatus.Cancelled)
            return new(status.DisplayName,
                $"A(z) #{orderId} rendelésed lemondásra került. Ha kérdésed van, keresd az éttermet.", null);

        return new(status.DisplayName, $"A(z) #{orderId} rendelésed új állapota: {status.DisplayName}.", null);
    }

    private static string BuildHtml(Order order, string subject, Content content)
    {
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html lang=\"hu\"><head><meta charset=\"utf-8\"><title>")
            .Append(E(subject))
            .Append("</title></head>")
            .Append("<body style=\"margin:0;padding:24px;background:#f5f5f5;font-family:Arial,Helvetica,sans-serif;color:#212529;\">")
            .Append("<div style=\"max-width:560px;margin:0 auto;background:#ffffff;border-radius:8px;padding:24px;\">")
            .Append("<h1 style=\"font-size:20px;margin:0 0 16px;\">").Append(E(content.Title)).Append("</h1>")
            .Append("<p>Kedves ").Append(E(order.CustomerName)).Append("!</p>")
            .Append("<p>").Append(E(content.Lead)).Append("</p>");

        if (content.CourierLine is not null)
        {
            html.Append("<p style=\"background:#e7f1fb;border-radius:6px;padding:12px;\"><strong>")
                .Append(E(content.CourierLine))
                .Append("</strong></p>");
        }

        html.Append("<h2 style=\"font-size:16px;margin:24px 0 8px;\">A rendelés részletei (#").Append(order.Id).Append(")</h2>")
            .Append("<table role=\"presentation\" style=\"width:100%;border-collapse:collapse;font-size:14px;\">");

        foreach (var item in order.Items)
        {
            html.Append("<tr><td style=\"padding:4px 0;\">")
                .Append(item.Quantity).Append(" × ").Append(E(item.DishName))
                .Append("</td><td style=\"padding:4px 0;text-align:right;white-space:nowrap;\">")
                .Append(E(HufFormatter.Format(item.LineTotal.Amount)))
                .Append("</td></tr>");
        }

        html.Append("<tr><td style=\"padding:8px 0 0;border-top:1px solid #dee2e6;font-weight:bold;\">Végösszeg</td>")
            .Append("<td style=\"padding:8px 0 0;border-top:1px solid #dee2e6;font-weight:bold;text-align:right;white-space:nowrap;\">")
            .Append(E(HufFormatter.Format(order.Total.Amount)))
            .Append("</td></tr></table>");

        html.Append("<p style=\"font-size:14px;line-height:1.5;\">Szállítási cím: ").Append(E(order.DeliveryAddress));
        if (!string.IsNullOrWhiteSpace(order.Note))
            html.Append("<br>Megjegyzés: ").Append(E(order.Note));
        html.Append("<br>Leadva: ").Append(E(HuDateFormatter.Format(order.PlacedAt))).Append("</p>");

        html.Append("<p style=\"font-size:12px;color:#6c757d;margin-top:24px;\">").Append(E(Footer)).Append("</p>")
            .Append("</div></body></html>");

        return html.ToString();
    }

    private static string BuildText(Order order, Content content)
    {
        var text = new StringBuilder();
        text.AppendLine($"Kedves {order.CustomerName}!")
            .AppendLine()
            .AppendLine(content.Lead);

        if (content.CourierLine is not null)
            text.AppendLine(content.CourierLine);

        text.AppendLine()
            .AppendLine($"A rendelés részletei (#{order.Id}):");
        foreach (var item in order.Items)
            text.AppendLine($"  {item.Quantity} × {item.DishName} – {HufFormatter.Format(item.LineTotal.Amount)}");

        text.AppendLine($"Végösszeg: {HufFormatter.Format(order.Total.Amount)}")
            .AppendLine($"Szállítási cím: {order.DeliveryAddress}");
        if (!string.IsNullOrWhiteSpace(order.Note))
            text.AppendLine($"Megjegyzés: {order.Note}");
        text.AppendLine($"Leadva: {HuDateFormatter.Format(order.PlacedAt)}")
            .AppendLine()
            .AppendLine(Footer);

        return text.ToString();
    }

    /// <summary>
    /// HTML-kódolás a szövegtartalomhoz és az attribútumokhoz: a öt különleges karaktert cseréli.
    /// A magyar ékezetes betűk változatlanok maradnak (a levél UTF-8 kódolású), így a forrás is olvasható.
    /// </summary>
    private static string E(string value) => value
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'", "&#39;");
}
