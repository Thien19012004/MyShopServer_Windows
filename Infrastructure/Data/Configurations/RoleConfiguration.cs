using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleName)
            .HasConversion<string>()   // enum -> string
            .HasMaxLength(20)
            .IsRequired();
    }
}
