using HotChocolate.Authorization;
using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Products;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Products;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ProductMutations
{
    // Chỉ Admin + Moderator được tạo sản phẩm
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<ProductResultDto> CreateProduct(
        CreateProductInput input,
        [Service] IProductService productService,
        CancellationToken ct)
    {
        try
        {
            var dto = new ProductDetailDto
            {
                Sku = input.Sku,
                Name = input.Name,
                ImportPrice = input.ImportPrice,
                SalePrice = input.SalePrice,
                StockQuantity = input.StockQuantity,
                Description = input.Description,
                CategoryId = input.CategoryId,
                ImagePaths = input.ImagePaths ?? new List<string>()
            };

            var created = await productService.CreateProductAsync(dto, ct);

            return new ProductResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "Product created successfully",
                Data = created
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

    // Chỉ Admin + Moderator được sửa sản phẩm
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<ProductResultDto> UpdateProduct(
        int productId,
        UpdateProductInput input,
        [Service] IProductService productService,
        CancellationToken ct)
    {
        try
        {
            var existing = await productService.GetProductByIdAsync(productId, ct);
            if (existing == null)
            {
                return new ProductResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Product not found",
                    Data = null
                };
            }

            var dto = new ProductDetailDto
            {
                ProductId = existing.ProductId,
                Sku = input.Sku ?? existing.Sku,
                Name = input.Name ?? existing.Name,
                ImportPrice = input.ImportPrice ?? existing.ImportPrice,
                SalePrice = input.SalePrice ?? existing.SalePrice,
                StockQuantity = input.StockQuantity ?? existing.StockQuantity,
                Description = input.Description ?? existing.Description,
                CategoryId = input.CategoryId ?? existing.CategoryId,
                CategoryName = existing.CategoryName,
                ImagePaths = input.ImagePaths ?? existing.ImagePaths
            };

            var updated = await productService.UpdateProductAsync(productId, dto, ct);

            return new ProductResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Product updated successfully",
                Data = updated
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

    // Chỉ Admin + Moderator được xoá sản phẩm
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<ProductResultDto> DeleteProduct(
        int productId,
        [Service] IProductService productService,
        CancellationToken ct)
    {
        try
        {
            var ok = await productService.DeleteProductAsync(productId, ct);
            if (!ok)
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
                Message = "Product deleted successfully",
                Data = null
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
