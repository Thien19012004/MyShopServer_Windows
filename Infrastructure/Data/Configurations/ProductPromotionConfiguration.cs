using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data.Configurations;

public class ProductPromotionConfiguration : IEntityTypeConfiguration<ProductPromotion>
{
    public void Configure(EntityTypeBuilder<ProductPromotion> builder)
    {
        builder.ToTable("ProductPromotions");

        // PK ghép
        builder.HasKey(pp => new { pp.ProductId, pp.PromotionId });

        // FK đã map trong ProductConfiguration & PromotionConfiguration
    }
}
