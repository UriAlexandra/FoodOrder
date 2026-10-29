using FoodOrder.Application.Notifications;

namespace FoodOrder.Application.Ports;

/// <summary>Külső e-mail-küldés (SMTP). Hibánál kivételt dob; az újrapróbálást az outbox végzi.</summary>
public interface IEmailGateway
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}
