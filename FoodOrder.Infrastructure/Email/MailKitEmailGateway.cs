using FoodOrder.Application.Notifications;
using FoodOrder.Application.Ports;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FoodOrder.Infrastructure.Email;

/// <summary>E-mail-küldés SMTP-n keresztül (MailKit). Hibánál kivételt dob, az újrapróbálást az outbox végzi.</summary>
public class MailKitEmailGateway(IOptions<SmtpOptions> options) : IEmailGateway
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    public async Task SendAsync(EmailMessage email, CancellationToken ct)
    {
        var smtp = options.Value;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        message.To.Add(new MailboxAddress(email.ToName, email.To));
        message.Subject = email.Subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = email.HtmlBody,
            TextBody = email.TextBody
        }.ToMessageBody();

        using var client = new SmtpClient { Timeout = (int)Timeout.TotalMilliseconds };

        // STARTTLS nélkül titkosítatlan a kapcsolat: csak helyi fejlesztői SMTP-hez (smtp4dev) használd.
        var security = smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(smtp.Host, smtp.Port, security, ct);

        if (!string.IsNullOrWhiteSpace(smtp.UserName))
            await client.AuthenticateAsync(smtp.UserName, smtp.Password ?? string.Empty, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }
}
