using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.LogId);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
