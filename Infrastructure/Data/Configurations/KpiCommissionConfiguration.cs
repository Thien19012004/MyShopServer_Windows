using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class KpiCommissionConfiguration : IEntityTypeConfiguration<KpiCommission>
{
    public void Configure(EntityTypeBuilder<KpiCommission> builder)
    {
        builder.ToTable("KpiCommissions");

        builder.HasKey(x => x.KpiCommissionId);

        builder.Property(x => x.Year)
         .IsRequired();

        builder.Property(x => x.Month)
             .IsRequired();

        builder.Property(x => x.BaseCommission)
         .IsRequired()
          .HasDefaultValue(0);

        builder.Property(x => x.BonusCommission)
 .IsRequired()
    .HasDefaultValue(0);

        builder.Property(x => x.TotalCommission)
  .IsRequired()
       .HasDefaultValue(0);

        builder.Property(x => x.TotalRevenue)
         .IsRequired()
                  .HasDefaultValue(0);

        builder.Property(x => x.TotalOrders)
       .IsRequired()
      .HasDefaultValue(0);

        builder.Property(x => x.CalculatedAt)
       .IsRequired();

        // FK
        builder.HasOne(x => x.Sale)
             .WithMany(u => u.KpiCommissions)
          .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.KpiTier)
            .WithMany(t => t.KpiCommissions)
             .HasForeignKey(x => x.KpiTierId)
          .OnDelete(DeleteBehavior.SetNull);

        // Unique: M?i sale ch? có 1 KPI commission record cho 1 tháng
        builder.HasIndex(x => new { x.SaleId, x.Year, x.Month })
         .IsUnique();
    }
}
