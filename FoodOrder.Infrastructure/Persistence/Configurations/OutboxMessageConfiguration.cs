using FoodOrder.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(OutboxMessage.MaxErrorLength);

        // A navigáció miatt új rendelésnél a mentéskor kapott OrderId kerül ide.
        builder.HasOne(x => x.Order)
               .WithMany()
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // A feldolgozó a függő bejegyzéseket keresi a létrehozás sorrendjében.
        builder.HasIndex(x => new { x.ProcessedAt, x.CreatedAt });
    }
}
