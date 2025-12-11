using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Categories;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class CategoryQueries
{
    public async Task<CategoryListResultDto> Categories(
        PaginationInput? pagination,
        string? search,  // optional filter theo tên
        [Service] ICategoryService categoryService,
        CancellationToken ct)
    {
        try
        {
            var options = new CategoryQueryOptions
            {
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize ?? 10,
                Search = search
            };

            var paged = await categoryService.GetAllAsync(options, ct);

            return new CategoryListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get categories success",
                Data = paged
            };
        }
        catch (Exception ex)
        {
            return new CategoryListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<CategoryResultDto> CategoryById(
        int categoryId,
        [Service] ICategoryService categoryService,
        CancellationToken ct)
    {
        try
        {
            var category = await categoryService.GetByIdAsync(categoryId, ct);

            if (category == null)
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
                Message = "Get category success",
                Data = category
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
