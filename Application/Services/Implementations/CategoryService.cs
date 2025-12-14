using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Categories;
using MyShopServer.DTOs.Common;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CategoryDto>> GetAllAsync(
    CategoryQueryOptions options,
    CancellationToken ct = default)
    {
        var page = options.Page <= 0 ? 1 : options.Page;
        var pageSize = options.PageSize <= 0 ? 10 : options.PageSize;

        var query = _db.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .AsQueryable();

        // Nếu KHÔNG search -> chạy DB như cũ
        if (string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.OrderBy(c => c.Name);

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                })
                .ToListAsync(ct);

            return new PagedResult<CategoryDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        // Có SEARCH -> lọc in-memory
        var term = TextSearch.Normalize(options.Search);

        // Load list thô
        var raw = await query
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .ToListAsync(ct);

        var filtered = raw
            .Where(c =>
            {
                var key = TextSearch.Normalize($"{c.Name} {c.Description}");
                return key.Contains(term);
            })
            .OrderBy(c => c.Name)
            .ToList();

        var total2 = filtered.Count;

        var items2 = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<CategoryDto>
        {
            Items = items2,
            TotalItems = total2,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int categoryId, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .SingleOrDefaultAsync(c => c.CategoryId == categoryId, ct);

        if (category == null)
            return null;

        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            ProductCount = category.Products.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CategoryDto dto, CancellationToken ct = default)
    {
        // Check trùng tên (optional)
        var exists = await _db.Categories
            .AnyAsync(c => c.Name == dto.Name, ct);

        if (exists)
        {
            throw new Exception($"Category with name '{dto.Name}' already exists.");
        }

        var entity = new Domain.Entities.Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CategoryDto
        {
            CategoryId = entity.CategoryId,
            Name = entity.Name,
            Description = entity.Description,
            ProductCount = 0
        };
    }

    public async Task<CategoryDto> UpdateAsync(int categoryId, CategoryDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Categories
            .Include(c => c.Products)
            .SingleOrDefaultAsync(c => c.CategoryId == categoryId, ct);

        if (entity == null)
            throw new Exception("Category not found");

        // optional: check trùng tên với category khác
        var nameExists = await _db.Categories
            .AnyAsync(c => c.CategoryId != categoryId && c.Name == dto.Name, ct);

        if (nameExists)
        {
            throw new Exception($"Category with name '{dto.Name}' already exists.");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await _db.SaveChangesAsync(ct);

        return new CategoryDto
        {
            CategoryId = entity.CategoryId,
            Name = entity.Name,
            Description = entity.Description,
            ProductCount = entity.Products.Count
        };
    }

    public async Task<bool> DeleteAsync(int categoryId, CancellationToken ct = default)
    {
        // Không cho xoá nếu có product
        var hasProducts = await _db.Products
            .AnyAsync(p => p.CategoryId == categoryId, ct);

        if (hasProducts)
        {
            throw new Exception("Cannot delete category because it has related products.");
        }

        var entity = await _db.Categories
            .SingleOrDefaultAsync(c => c.CategoryId == categoryId, ct);

        if (entity == null)
            return false;

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
