using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Promotions;
using MyShopServer.Domain.Enums;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class PromotionService : IPromotionService
{
    private readonly AppDbContext _db;

    public PromotionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PromotionDto>> GetPromotionsAsync(PromotionQueryOptions options, CancellationToken ct = default)
    {
        var page = Math.Max(1, options.Page);
        var pageSize = Math.Clamp(options.PageSize, 1, 100);

        var q = _db.Promotions
          .AsNoTracking()
       .Include(p => p.ProductPromotions)
             .Include(p => p.CategoryPromotions)
             .AsQueryable();

        if (options.OnlyActive)
        {
            var at = options.At ?? DateTime.UtcNow;
            q = q.Where(p => p.StartDate <= at && at <= p.EndDate);
        }

        // Filter by scope
        if (options.Scope.HasValue)
        {
            q = q.Where(p => p.Scope == options.Scope.Value);
        }

        if (string.IsNullOrWhiteSpace(options.Search))
        {
            q = q.OrderByDescending(p => p.StartDate).ThenByDescending(p => p.PromotionId);

            var total = await q.CountAsync(ct);

            var items = await q
   .Skip((page - 1) * pageSize)
              .Take(pageSize)
  .Select(p => new PromotionDto
  {
      PromotionId = p.PromotionId,
      Name = p.Name,
      DiscountPercent = p.DiscountPercent,
      StartDate = p.StartDate,
      EndDate = p.EndDate,
      Scope = p.Scope,
      ProductCount = p.ProductPromotions.Count,
      ProductIds = p.ProductPromotions.Select(x => x.ProductId).ToList(),
      CategoryCount = p.CategoryPromotions.Count,
      CategoryIds = p.CategoryPromotions.Select(x => x.CategoryId).ToList()
  })
          .ToListAsync(ct);

            // Clean up response based on scope
            foreach (var item in items)
            {
                CleanPromotionDtoByScope(item);
            }

            return new PagedResult<PromotionDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };
        }

        var term = TextSearch.Normalize(options.Search);

        var raw = await q
   .Select(p => new PromotionDto
   {
       PromotionId = p.PromotionId,
       Name = p.Name,
       DiscountPercent = p.DiscountPercent,
       StartDate = p.StartDate,
       EndDate = p.EndDate,
       Scope = p.Scope,
       ProductCount = p.ProductPromotions.Count,
       ProductIds = p.ProductPromotions.Select(x => x.ProductId).ToList(),
       CategoryCount = p.CategoryPromotions.Count,
       CategoryIds = p.CategoryPromotions.Select(x => x.CategoryId).ToList()
   })
            .ToListAsync(ct);

        var filtered = raw
 .Where(x => TextSearch.Normalize(x.Name).Contains(term))
            .OrderByDescending(x => x.StartDate)
   .ThenByDescending(x => x.PromotionId)
            .ToList();

        // Clean up response based on scope
        foreach (var item in filtered)
        {
            CleanPromotionDtoByScope(item);
        }

        var total2 = filtered.Count;
        var items2 = filtered
  .Skip((page - 1) * pageSize)
      .Take(pageSize)
            .ToList();

        return new PagedResult<PromotionDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total2,
            Items = items2
        };
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(int promotionId, CancellationToken ct = default)
    {
        var p = await _db.Promotions
      .AsNoTracking()
      .Include(x => x.ProductPromotions)
            .Include(x => x.CategoryPromotions)
       .SingleOrDefaultAsync(x => x.PromotionId == promotionId, ct);

        if (p == null) return null;

        var dto = new PromotionDto
        {
            PromotionId = p.PromotionId,
            Name = p.Name,
            DiscountPercent = p.DiscountPercent,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Scope = p.Scope,
            ProductCount = p.ProductPromotions.Count,
            ProductIds = p.ProductPromotions.Select(x => x.ProductId).ToList(),
            CategoryCount = p.CategoryPromotions.Count,
            CategoryIds = p.CategoryPromotions.Select(x => x.CategoryId).ToList()
        };

        // Clean up response based on scope
        CleanPromotionDtoByScope(dto);

        return dto;
    }

    public async Task<PromotionDto> CreatePromotionAsync(PromotionDto dto, CancellationToken ct = default)
    {
        ValidatePromotion(dto, isCreate: true);

        var entity = new Domain.Entities.Promotion
        {
            Name = dto.Name.Trim(),
            DiscountPercent = dto.DiscountPercent,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Scope = dto.Scope
        };

        // Validate scope và assign relations theo scope
        await AssignPromotionRelationsAsync(entity, dto, ct);

        _db.Promotions.Add(entity);
        await _db.SaveChangesAsync(ct);

        // Reload relations
        await _db.Entry(entity).Collection(x => x.ProductPromotions).LoadAsync(ct);
        await _db.Entry(entity).Collection(x => x.CategoryPromotions).LoadAsync(ct);

        var result = new PromotionDto
        {
            PromotionId = entity.PromotionId,
            Name = entity.Name,
            DiscountPercent = entity.DiscountPercent,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Scope = entity.Scope,
            ProductCount = entity.ProductPromotions.Count,
            ProductIds = entity.ProductPromotions.Select(x => x.ProductId).ToList(),
            CategoryCount = entity.CategoryPromotions.Count,
            CategoryIds = entity.CategoryPromotions.Select(x => x.CategoryId).ToList()
        };

        // Clean up response based on scope
        CleanPromotionDtoByScope(result);

        return result;
    }

    public async Task<PromotionDto?> UpdatePromotionAsync(int promotionId, PromotionDto dto, bool replaceProducts, CancellationToken ct = default)
    {
        var entity = await _db.Promotions
    .Include(x => x.ProductPromotions)
       .Include(x => x.CategoryPromotions)
   .SingleOrDefaultAsync(x => x.PromotionId == promotionId, ct);

        if (entity == null) return null;

        var now = DateTime.UtcNow;

        // Check if promotion is currently active
        var isActive = entity.StartDate <= now && now <= entity.EndDate;
        if (isActive)
        {
            throw new Exception("Cannot update an active promotion. Please wait until it expires or create a new one.");
        }

        // Don't allow setting startDate to the past
        if (dto.StartDate < now.Date)
        {
            throw new Exception("Cannot set StartDate to a past date.");
        }

        ValidatePromotion(dto, isCreate: false);

        entity.Name = dto.Name.Trim();
        entity.DiscountPercent = dto.DiscountPercent;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Scope = dto.Scope;

        // Clear old relations and assign new ones based on scope
        _db.ProductPromotions.RemoveRange(entity.ProductPromotions);
        _db.CategoryPromotions.RemoveRange(entity.CategoryPromotions);

        await AssignPromotionRelationsAsync(entity, dto, ct);

        await _db.SaveChangesAsync(ct);

        var result = new PromotionDto
        {
            PromotionId = entity.PromotionId,
            Name = entity.Name,
            DiscountPercent = entity.DiscountPercent,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Scope = entity.Scope,
            ProductCount = entity.ProductPromotions.Count,
            ProductIds = entity.ProductPromotions.Select(x => x.ProductId).ToList(),
            CategoryCount = entity.CategoryPromotions.Count,
            CategoryIds = entity.CategoryPromotions.Select(x => x.CategoryId).ToList()
        };


        // Clean up response based on scope
        CleanPromotionDtoByScope(result);

        return result;
    }

    public async Task<bool> DeletePromotionAsync(int promotionId, CancellationToken ct = default)
    {
        var entity = await _db.Promotions
            .Include(x => x.ProductPromotions)
         .Include(x => x.CategoryPromotions)
  .Include(x => x.OrderPromotions)
     .SingleOrDefaultAsync(x => x.PromotionId == promotionId, ct);

        if (entity == null) return false;

        _db.ProductPromotions.RemoveRange(entity.ProductPromotions);
        _db.CategoryPromotions.RemoveRange(entity.CategoryPromotions);
        _db.OrderPromotions.RemoveRange(entity.OrderPromotions);
        _db.Promotions.Remove(entity);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> GetBestDiscountPercentAsync(int productId, DateTime at, CancellationToken ct = default)
    {
        var best = await _db.ProductPromotions
                   .AsNoTracking()
                   .Where(pp => pp.ProductId == productId)
                   .Select(pp => pp.Promotion)
                   .Where(p => p.StartDate <= at && at <= p.EndDate && p.Scope == PromotionScope.Product)
                   .Select(p => (int?)p.DiscountPercent)
                   .MaxAsync(ct);

        return best ?? 0;
    }

    private async Task AssignPromotionRelationsAsync(Domain.Entities.Promotion entity, PromotionDto dto, CancellationToken ct)
    {
        switch (dto.Scope)
        {
            case PromotionScope.Product:
                var productIds = (dto.ProductIds ?? new List<int>()).Distinct().ToList();
                if (productIds.Count == 0)
                    throw new Exception("Product scope promotion must have at least one product.");

                var existProductCount = await _db.Products.CountAsync(x => productIds.Contains(x.ProductId), ct);
                if (existProductCount != productIds.Count)
                    throw new Exception("Some ProductIds do not exist.");

                entity.ProductPromotions = productIds
                  .Select(pid => new Domain.Entities.ProductPromotion
                  {
                      ProductId = pid,
                      PromotionId = entity.PromotionId
                  })
                 .ToList();
                break;

            case PromotionScope.Category:
                var categoryIds = (dto.CategoryIds ?? new List<int>()).Distinct().ToList();
                if (categoryIds.Count == 0)
                    throw new Exception("Category scope promotion must have at least one category.");

                var existCategoryCount = await _db.Categories.CountAsync(x => categoryIds.Contains(x.CategoryId), ct);
                if (existCategoryCount != categoryIds.Count)
                    throw new Exception("Some CategoryIds do not exist.");

                entity.CategoryPromotions = categoryIds
       .Select(cid => new Domain.Entities.CategoryPromotion
       {
           CategoryId = cid,
           PromotionId = entity.PromotionId
       })
                    .ToList();
                break;

            case PromotionScope.Order:
                // Order scope doesn't need product or category assignment
                // Only validate that productIds or categoryIds are not passed
                if ((dto.ProductIds != null && dto.ProductIds.Count > 0) ||
        (dto.CategoryIds != null && dto.CategoryIds.Count > 0))
                {
                    throw new Exception("Order scope promotion should not have ProductIds or CategoryIds.");
                }
                break;

            default:
                throw new Exception("Invalid promotion scope.");
        }
    }

    private static void ValidatePromotion(PromotionDto dto, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Promotion name is required.");

        if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
            throw new Exception("DiscountPercent must be between 0 and 100.");

        if (dto.EndDate < dto.StartDate)
            throw new Exception("EndDate must be greater than or equal to StartDate.");

        if (isCreate && dto.StartDate < DateTime.UtcNow.Date)
            throw new Exception("Cannot create promotion with StartDate in the past.");
    }

    // Helper method to clean DTO based on scope
    private static void CleanPromotionDtoByScope(PromotionDto dto)
    {
        switch (dto.Scope)
        {
            case PromotionScope.Product:
                // Only show product-related data
                dto.CategoryCount = 0;
                dto.CategoryIds = new List<int>();
                break;

            case PromotionScope.Category:
                // Only show category-related data
                dto.ProductCount = 0;
                dto.ProductIds = new List<int>();
                break;

            case PromotionScope.Order:
                // Don't show product or category data
                dto.ProductCount = 0;
                dto.ProductIds = new List<int>();
                dto.CategoryCount = 0;
                dto.CategoryIds = new List<int>();
                break;
        }
    }
}
