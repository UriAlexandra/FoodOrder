namespace FoodOrder.Application.Notifications;

/// <summary>Elküldendő levél: címzett, tárgy, HTML és sima szöveges törzs.</summary>
public sealed record EmailMessage(string To, string ToName, string Subject, string HtmlBody, string TextBody);
