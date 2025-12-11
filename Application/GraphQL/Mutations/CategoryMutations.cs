using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Categories;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Categories;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CategoryMutations
{
    public async Task<CategoryResultDto> CreateCategory(
        CreateCategoryInput input,
        [Service] ICategoryService categoryService,
        CancellationToken ct)
    {
        try
        {
            var dto = new CategoryDto
            {
                Name = input.Name,
                Description = input.Description
            };

            var created = await categoryService.CreateAsync(dto, ct);

            return new CategoryResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "Category created successfully",
                Data = created
            };
        }
        catch (Exception ex)
        {
            return new CategoryResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<CategoryResultDto> UpdateCategory(
        int categoryId,
        UpdateCategoryInput input,
        [Service] ICategoryService categoryService,
        CancellationToken ct)
    {
        try
        {
            var existing = await categoryService.GetByIdAsync(categoryId, ct);
            if (existing == null)
            {
                return new CategoryResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Category not found",
                    Data = null
                };
            }

            var dto = new CategoryDto
            {
                CategoryId = existing.CategoryId,
                Name = input.Name ?? existing.Name,
                Description = input.Description ?? existing.Description,
                ProductCount = existing.ProductCount
            };

            var updated = await categoryService.UpdateAsync(categoryId, dto, ct);

            return new CategoryResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Category updated successfully",
                Data = updated
            };
        }
        catch (Exception ex)
        {
            return new CategoryResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<CategoryResultDto> DeleteCategory(
        int categoryId,
        [Service] ICategoryService categoryService,
        CancellationToken ct)
    {
        try
        {
            var deleted = await categoryService.DeleteAsync(categoryId, ct);
            if (!deleted)
            {
                return new CategoryResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Category not found",
                    Data = null
                };
            }

            return new CategoryResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Category deleted successfully",
                Data = null
            };
        }
        catch (Exception ex)
        {
            return new CategoryResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
