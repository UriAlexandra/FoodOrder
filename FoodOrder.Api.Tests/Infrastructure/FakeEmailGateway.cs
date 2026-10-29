using System.Collections.Concurrent;
using FoodOrder.Application.Notifications;
using FoodOrder.Application.Ports;

namespace FoodOrder.Api.Tests.Infrastructure;

/// <summary>A valódi SMTP-küldés helyett rögzíti a leveleket, a tesztek ezt vizsgálják.</summary>
public sealed class FakeEmailGateway : IEmailGateway
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();

    public IReadOnlyCollection<EmailMessage> Sent => _sent.ToArray();

    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        _sent.Enqueue(message);
        return Task.CompletedTask;
    }
}
