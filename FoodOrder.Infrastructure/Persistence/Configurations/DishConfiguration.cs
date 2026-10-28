using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Persistence.Configurations;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("Dishes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);

        builder.ComplexProperty(x => x.Price, price =>
        {
            price.Property(m => m.Amount).HasColumnName("Price").HasPrecision(18, 2);
            price.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });
    }
}
