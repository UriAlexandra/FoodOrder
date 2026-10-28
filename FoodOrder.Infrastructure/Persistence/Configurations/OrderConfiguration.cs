using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CustomerEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DeliveryAddress).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(300);
        builder.Property(x => x.Status).IsRequired();

        // Optimista zárolás: a Version minden módosításkor nő, a mentés ellenőrzi.
        builder.Property(x => x.Version).IsConcurrencyToken();

        // A végösszeg a tételekből számolódik, nem tároljuk.
        builder.Ignore(x => x.Total);

        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey("OrderId")
               .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Order.Items))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.StatusHistory)
               .WithOne()
               .HasForeignKey("OrderId")
               .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Order.StatusHistory))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
