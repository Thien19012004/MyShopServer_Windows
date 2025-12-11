using System.Security.Claims;
using HotChocolate.Types;
using MyShopServer.DTOs.Products;

namespace MyShopServer.Application.GraphQL.Types;

public class ProductDetailType : ObjectType<ProductDetailDto>
{
    protected override void Configure(IObjectTypeDescriptor<ProductDetailDto> descriptor)
    {
        descriptor.Field(p => p.ProductId);
        descriptor.Field(p => p.Sku);
        descriptor.Field(p => p.Name);
        descriptor.Field(p => p.SalePrice);
        descriptor.Field(p => p.StockQuantity);
        descriptor.Field(p => p.Description);
        descriptor.Field(p => p.CategoryId);
        descriptor.Field(p => p.CategoryName);
        descriptor.Field(p => p.ImagePaths);

        descriptor
            .Field(p => p.ImportPrice)
            .ResolveWith<ProductPriceResolvers>(r =>
                r.GetImportPrice(default!, default!))
            .Description("Import price visible only for Admin/Moderator");
    }
    private sealed class ProductPriceResolvers
    {
        public int GetImportPrice(
            [Parent] ProductDetailDto dto,
            ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin") || user.IsInRole("Moderator"))
            {
                return dto.ImportPrice;
            }

            // Sale: ẩn giá nhập
            return 0;
        }
    }
}
