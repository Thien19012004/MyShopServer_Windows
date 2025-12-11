using MyShopServer.DTOs.Categories;
using MyShopServer.DTOs.Common;

namespace MyShopServer.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetAllAsync(CategoryQueryOptions options, CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(int categoryId, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CategoryDto dto, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(int categoryId, CategoryDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int categoryId, CancellationToken ct = default);
}
