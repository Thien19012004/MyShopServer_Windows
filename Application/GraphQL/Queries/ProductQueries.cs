using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Products;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Products;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ProductQueries
{
    public async Task<ProductListResultDto> Products(
        PaginationInput? pagination,
        ProductFilterInput? filter,
        ProductSortInput? sort,
        [Service] IProductService productService,
        CancellationToken ct)
    {
        try
        {
            var options = new ProductQueryOptions
            {
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize ?? 10,
                CategoryId = filter?.CategoryId,
                Search = filter?.Search,
                MinPrice = filter?.MinPrice,
                MaxPrice = filter?.MaxPrice,
                SortBy = sort?.Field ?? ProductSortBy.Name,
                SortAsc = sort?.Asc ?? true
            };

            var paged = await productService.GetProductsAsync(options, ct);

            return new ProductListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get products success",
                Data = paged
            };
        }
        catch (Exception ex)
        {
            return new ProductListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<ProductResultDto> ProductById(
        int productId,
        [Service] IProductService productService,
        CancellationToken ct)
    {
        try
        {
            var product = await productService.GetProductByIdAsync(productId, ct);

            if (product == null)
            {
                return new ProductResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Product not found",
                    Data = null
                };
            }

            return new ProductResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get product success",
                Data = product
            };
        }
        catch (Exception ex)
        {
            return new ProductResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

}
