using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("Commissions");

        builder.HasKey(c => c.CommissionId);

        builder.Property(c => c.Amount)
            .IsRequired();

        builder.Property(c => c.CalculatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 1 Commission - 1 Order (1-1)
        builder.HasOne(c => c.Order)
            .WithOne()                            // không có nav bên Order
            .HasForeignKey<Commission>(c => c.OrderId);
    }
}
