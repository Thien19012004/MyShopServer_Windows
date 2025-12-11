using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");

        builder.HasKey(s => s.Key);

        builder.Property(s => s.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Value)
            .HasMaxLength(500);
    }
}
