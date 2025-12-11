using System.Security.Claims;
using HotChocolate.Types;
using MyShopServer.DTOs.Products;

namespace MyShopServer.Application.GraphQL.Types;

public class ProductListItemType : ObjectType<ProductListItemDto>
{
    protected override void Configure(IObjectTypeDescriptor<ProductListItemDto> descriptor)
    {
        descriptor.Field(p => p.ProductId);
        descriptor.Field(p => p.Sku);
        descriptor.Field(p => p.Name);
        descriptor.Field(p => p.SalePrice);
        descriptor.Field(p => p.StockQuantity);
        descriptor.Field(p => p.CategoryName);

        descriptor
            .Field(p => p.ImportPrice)
            .ResolveWith<ProductPriceResolvers>(r =>
                r.GetImportPrice(default!, default!))
            .Description("Import price visible only for Admin/Moderator");
    }

    private sealed class ProductPriceResolvers
    {
        public int GetImportPrice(
            [Parent] ProductListItemDto dto,
            ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin") || user.IsInRole("Moderator"))
            {
                return dto.ImportPrice;
            }

            return 0;
        }
    }
}
