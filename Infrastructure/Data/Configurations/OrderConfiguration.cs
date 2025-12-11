using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders"); // tránh trùng keyword ORDER

        builder.HasKey(o => o.OrderId);

        builder.Property(o => o.Status)
            .HasConversion<string>()  // Created / Paid / Cancelled
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.TotalPrice)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Quan hệ tới Sale (User) đã được cấu hình ở UserConfiguration
        // Quan hệ tới Customer đã config ở CustomerConfiguration

        // 1 Order - N OrderItems
        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);

        // 1 Order - N Payments
        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId);
    }
}
