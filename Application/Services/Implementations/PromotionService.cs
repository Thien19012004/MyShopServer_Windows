using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Common; // nếu bạn muốn dùng TextSearch.Normalize (optional)
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Promotions;
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
            .AsQueryable();

        if (options.OnlyActive)
        {
            var at = options.At ?? DateTime.UtcNow;
            q = q.Where(p => p.StartDate <= at && at <= p.EndDate);
        }

        // Search accent-insensitive kiểu “in-memory” (giữ DB) — giống bạn đang làm Product/Category
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
                    ProductCount = p.ProductPromotions.Count,
                    ProductIds = p.ProductPromotions.Select(x => x.ProductId).ToList()
                })
                .ToListAsync(ct);

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
                ProductCount = p.ProductPromotions.Count,
                ProductIds = p.ProductPromotions.Select(x => x.ProductId).ToList()
            })
            .ToListAsync(ct);

        var filtered = raw
            .Where(x => TextSearch.Normalize(x.Name).Contains(term))
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.PromotionId)
            .ToList();

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
            .SingleOrDefaultAsync(x => x.PromotionId == promotionId, ct);

        if (p == null) return null;

        return new PromotionDto
        {
            PromotionId = p.PromotionId,
            Name = p.Name,
            DiscountPercent = p.DiscountPercent,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            ProductCount = p.ProductPromotions.Count,
            ProductIds = p.ProductPromotions.Select(x => x.ProductId).ToList()
        };
    }

    public async Task<PromotionDto> CreatePromotionAsync(PromotionDto dto, CancellationToken ct = default)
    {
        ValidatePromotion(dto);

        var entity = new Domain.Entities.Promotion
        {
            Name = dto.Name.Trim(),
            DiscountPercent = dto.DiscountPercent,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
        };

        // gán sản phẩm
        var productIds = (dto.ProductIds ?? new List<int>()).Distinct().ToList();
        if (productIds.Count > 0)
        {
            var existCount = await _db.Products.CountAsync(x => productIds.Contains(x.ProductId), ct);
            if (existCount != productIds.Count)
                throw new Exception("Some ProductIds do not exist.");

            entity.ProductPromotions = productIds
                .Select(pid => new Domain.Entities.ProductPromotion
                {
                    ProductId = pid
                })
                .ToList();
        }

        _db.Promotions.Add(entity);
        await _db.SaveChangesAsync(ct);

        // reload join
        await _db.Entry(entity).Collection(x => x.ProductPromotions).LoadAsync(ct);

        return new PromotionDto
        {
            PromotionId = entity.PromotionId,
            Name = entity.Name,
            DiscountPercent = entity.DiscountPercent,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            ProductCount = entity.ProductPromotions.Count,
            ProductIds = entity.ProductPromotions.Select(x => x.ProductId).ToList()
        };
    }

    public async Task<PromotionDto?> UpdatePromotionAsync(int promotionId, PromotionDto dto, bool replaceProducts, CancellationToken ct = default)
    {
        ValidatePromotion(dto);

        var entity = await _db.Promotions
            .Include(x => x.ProductPromotions)
            .SingleOrDefaultAsync(x => x.PromotionId == promotionId, ct);

        if (entity == null) return null;

        entity.Name = dto.Name.Trim();
        entity.DiscountPercent = dto.DiscountPercent;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;

        if (replaceProducts)
        {
            var productIds = (dto.ProductIds ?? new List<int>()).Distinct().ToList();

            var existCount = await _db.Products.CountAsync(x => productIds.Contains(x.ProductId), ct);
            if (existCount != productIds.Count)
                throw new Exception("Some ProductIds do not exist.");

            // replace list
            _db.ProductPromotions.RemoveRange(entity.ProductPromotions);
            entity.ProductPromotions = productIds
                .Select(pid => new Domain.Entities.ProductPromotion
                {
                    ProductId = pid,
                    PromotionId = entity.PromotionId
                })
                .ToList();
        }

        await _db.SaveChangesAsync(ct);

        return new PromotionDto
        {
            PromotionId = entity.PromotionId,
            Name = entity.Name,
            DiscountPercent = entity.DiscountPercent,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            ProductCount = entity.ProductPromotions.Count,
            ProductIds = entity.ProductPromotions.Select(x => x.ProductId).ToList()
        };
    }

    public async Task<bool> DeletePromotionAsync(int promotionId, CancellationToken ct = default)
    {
        var entity = await _db.Promotions
            .Include(x => x.ProductPromotions)
            .SingleOrDefaultAsync(x => x.PromotionId == promotionId, ct);

        if (entity == null) return false;

        _db.ProductPromotions.RemoveRange(entity.ProductPromotions);
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
            .Where(p => p.StartDate <= at && at <= p.EndDate)
            .Select(p => (int?)p.DiscountPercent)
            .MaxAsync(ct);

        return best ?? 0;
    }

    private static void ValidatePromotion(PromotionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Promotion name is required.");

        if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
            throw new Exception("DiscountPercent must be between 0 and 100.");

        if (dto.EndDate < dto.StartDate)
            throw new Exception("EndDate must be greater than or equal to StartDate.");
    }
}
