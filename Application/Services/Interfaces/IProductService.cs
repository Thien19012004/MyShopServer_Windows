using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Products;

namespace MyShopServer.Application.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductQueryOptions options, CancellationToken ct = default);
    Task<ProductDetailDto?> GetProductByIdAsync(int productId, CancellationToken ct = default);
    Task<ProductDetailDto> CreateProductAsync(ProductDetailDto dto, CancellationToken ct = default);
    Task<ProductDetailDto> UpdateProductAsync(int productId, ProductDetailDto dto, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(int productId, CancellationToken ct = default);
}
