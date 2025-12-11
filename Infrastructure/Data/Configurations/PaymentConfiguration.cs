using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.Method)
            .HasConversion<string>()   // Cash, BankTransfer, ...
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
