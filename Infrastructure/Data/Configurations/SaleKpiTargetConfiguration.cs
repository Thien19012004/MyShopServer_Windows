using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class SaleKpiTargetConfiguration : IEntityTypeConfiguration<SaleKpiTarget>
{
    public void Configure(EntityTypeBuilder<SaleKpiTarget> builder)
    {
        builder.ToTable("SaleKpiTargets");

        builder.HasKey(x => x.SaleKpiTargetId);

        builder.Property(x => x.Year)
               .IsRequired();

        builder.Property(x => x.Month)
              .IsRequired();

        builder.Property(x => x.TargetRevenue)
            .IsRequired();

        builder.Property(x => x.ActualRevenue)
              .IsRequired()
          .HasDefaultValue(0);

        builder.Property(x => x.BonusAmount)
   .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // FK
        builder.HasOne(x => x.Sale)
  .WithMany(u => u.SaleKpiTargets)
   .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.KpiTier)
         .WithMany(t => t.SaleKpiTargets)
          .HasForeignKey(x => x.KpiTierId)
      .OnDelete(DeleteBehavior.SetNull);

        // Unique: mỗi sale chỉ có 1 target cho 1 tháng
        builder.HasIndex(x => new { x.SaleId, x.Year, x.Month })
                .IsUnique();
    }
}
