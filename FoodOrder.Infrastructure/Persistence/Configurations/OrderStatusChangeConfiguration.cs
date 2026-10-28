using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Persistence.Configurations;

public class OrderStatusChangeConfiguration : IEntityTypeConfiguration<OrderStatusChange>
{
    public void Configure(EntityTypeBuilder<OrderStatusChange> builder)
    {
        builder.ToTable("OrderStatusChanges");
        builder.HasKey(x => x.Id);

        // A FromStatus/ToStatus átalakítását az AppDbContext konvenciója adja (OrderStatusConverter);
        // a FromStatus az első bejegyzésnél üres, ezért nem kötelező.
        builder.Property(x => x.ToStatus).IsRequired();
        builder.Property(x => x.ChangedBy).HasMaxLength(256).IsRequired();
    }
}
