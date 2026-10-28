using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Persistence.Configurations;

public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.ToTable("Couriers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
    }
}
