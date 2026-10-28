using FoodOrder.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FoodOrder.Infrastructure.Persistence.Converters;

/// <summary>
/// OrderStatus &lt;-&gt; szöveges kód (pl. "Placed") átalakítás az adatbázis felé.
/// </summary>
public sealed class OrderStatusConverter : ValueConverter<OrderStatus, string>
{
    public OrderStatusConverter()
        : base(status => status.Code, code => OrderStatus.FromCode(code))
    {
    }
}
