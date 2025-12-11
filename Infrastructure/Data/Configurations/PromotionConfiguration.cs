using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(p => p.PromotionId);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.DiscountPercent)
            .IsRequired();

        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.EndDate).IsRequired();

        builder.HasMany(p => p.ProductPromotions)
            .WithOne(pp => pp.Promotion)
            .HasForeignKey(pp => pp.PromotionId);
    }
}
