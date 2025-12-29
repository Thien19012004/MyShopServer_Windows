using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;

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

        builder.Property(p => p.Scope)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasMany(p => p.ProductPromotions)
            .WithOne(pp => pp.Promotion)
            .HasForeignKey(pp => pp.PromotionId);

        builder.HasMany(p => p.CategoryPromotions)
            .WithOne(cp => cp.Promotion)
            .HasForeignKey(cp => cp.PromotionId);

        builder.HasMany(p => p.OrderPromotions)
            .WithOne(op => op.Promotion)
            .HasForeignKey(op => op.PromotionId);
    }
}
