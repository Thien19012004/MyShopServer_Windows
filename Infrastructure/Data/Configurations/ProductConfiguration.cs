using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.ImportPrice)
            .IsRequired();

        builder.Property(p => p.SalePrice)
            .IsRequired();

        builder.Property(p => p.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Quan hệ tới ProductImage
        builder.HasMany(p => p.Images)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ tới OrderItem
        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId);

        // Quan hệ tới ProductPromotion (N-N)
        builder.HasMany(p => p.ProductPromotions)
            .WithOne(pp => pp.Product)
            .HasForeignKey(pp => pp.ProductId);
    }
}
