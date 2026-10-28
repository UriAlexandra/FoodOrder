using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DishName).HasMaxLength(80).IsRequired();

        // A sorösszeg (egységár × mennyiség) számolt érték, nem tároljuk.
        builder.Ignore(x => x.LineTotal);

        builder.ComplexProperty(x => x.UnitPrice, price =>
        {
            price.Property(m => m.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
            price.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });
    }
}
