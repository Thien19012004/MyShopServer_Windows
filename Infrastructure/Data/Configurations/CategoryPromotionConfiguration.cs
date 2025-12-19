using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class CategoryPromotionConfiguration : IEntityTypeConfiguration<CategoryPromotion>
{
    public void Configure(EntityTypeBuilder<CategoryPromotion> builder)
    {
        builder.ToTable("CategoryPromotions");

        builder.HasKey(x => new { x.CategoryId, x.PromotionId });

        builder.HasOne(x => x.Category)
            .WithMany(c => c.CategoryPromotions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Promotion)
            .WithMany(p => p.CategoryPromotions)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
