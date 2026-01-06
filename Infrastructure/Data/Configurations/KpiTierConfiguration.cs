using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class KpiTierConfiguration : IEntityTypeConfiguration<KpiTier>
{
    public void Configure(EntityTypeBuilder<KpiTier> builder)
    {
        builder.ToTable("KpiTiers");

        builder.HasKey(x => x.KpiTierId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.MinRevenue)
            .IsRequired();

        builder.Property(x => x.BonusPercent)
      .IsRequired();

        builder.Property(x => x.Description)
     .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
 .IsRequired();

        // Index
        builder.HasIndex(x => x.DisplayOrder);
    }
}
