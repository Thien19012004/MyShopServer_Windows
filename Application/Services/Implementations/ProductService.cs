using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Products;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly IPromotionService _promotion;

    public ProductService(AppDbContext db, IPromotionService promotion)
    {
        _db = db;
        _promotion = promotion;
    }

    private async Task<Dictionary<int, int>> GetBestDiscountMapAsync(
    IReadOnlyCollection<int> productIds,
    DateTime at,
    CancellationToken ct)
    {
        if (productIds.Count == 0) return new Dictionary<int, int>();

        return await _db.ProductPromotions
            .AsNoTracking()
            .Where(pp => productIds.Contains(pp.ProductId))
            .Select(pp => new
            {
                pp.ProductId,
                pp.Promotion.DiscountPercent,
                pp.Promotion.StartDate,
                pp.Promotion.EndDate
            })
            .Where(x => x.StartDate <= at && at <= x.EndDate)
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Best = g.Max(t => t.DiscountPercent) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Best, ct);
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(
    ProductQueryOptions options,
    CancellationToken ct = default)
    {
        var page = options.Page <= 0 ? 1 : options.Page;
        var pageSize = options.PageSize <= 0 ? 10 : options.PageSize;

        // Base query: filter “cứng” chạy DB cho nhẹ
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (options.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == options.CategoryId.Value);

        if (options.MinPrice.HasValue)
            query = query.Where(p => p.SalePrice >= options.MinPrice.Value);

        if (options.MaxPrice.HasValue)
            query = query.Where(p => p.SalePrice <= options.MaxPrice.Value);

        // Nếu KHÔNG search -> giữ nguyên sort + paging trong DB (tối ưu)
        if (string.IsNullOrWhiteSpace(options.Search))
        {
            query = (options.SortBy, options.SortAsc) switch
            {
                (ProductSortBy.SalePrice, true) => query.OrderBy(p => p.SalePrice),
                (ProductSortBy.SalePrice, false) => query.OrderByDescending(p => p.SalePrice),

                (ProductSortBy.ImportPrice, true) => query.OrderBy(p => p.ImportPrice),
                (ProductSortBy.ImportPrice, false) => query.OrderByDescending(p => p.ImportPrice),

                (ProductSortBy.StockQuantity, true) => query.OrderBy(p => p.StockQuantity),
                (ProductSortBy.StockQuantity, false) => query.OrderByDescending(p => p.StockQuantity),

                (ProductSortBy.CreatedAt, true) => query.OrderBy(p => p.CreatedAt),
                (ProductSortBy.CreatedAt, false) => query.OrderByDescending(p => p.CreatedAt),

                (_, true) => query.OrderBy(p => p.Name),
                (_, false) => query.OrderByDescending(p => p.Name),
            };

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListItemDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    Name = p.Name,
                    ImportPrice = p.ImportPrice,
                    SalePrice = p.SalePrice,
                    StockQuantity = p.StockQuantity,
                    CategoryName = p.Category!.Name
                })
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            var ids = items.Select(x => x.ProductId).ToList();
            var discountMap = await GetBestDiscountMapAsync(ids, now, ct);

            foreach (var p in items)
            {
                var pct = discountMap.TryGetValue(p.ProductId, out var best) ? best : 0;
                p.DiscountPct = pct;
                p.FinalPrice = PriceCalc.ApplyDiscount(p.SalePrice, pct);
            }


            return new PagedResult<ProductListItemDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        // Có SEARCH -> dùng Normalize() lọc in-memory
        var term = TextSearch.Normalize(options.Search);

        // Lấy candidate list sau khi đã filter cứng
        var raw = await query
            .Select(p => new
            {
                p.ProductId,
                p.Sku,
                p.Name,
                p.ImportPrice,
                p.SalePrice,
                p.StockQuantity,
                CategoryName = p.Category!.Name
            })
            .ToListAsync(ct);

        // Filter bằng key normalize: match name + sku
        var filtered = raw
            .Where(p =>
            {
                var key = TextSearch.Normalize($"{p.Name} {p.Sku}");
                return key.Contains(term);
            });

        // Sort in-memory
        filtered = (options.SortBy, options.SortAsc) switch
        {
            (ProductSortBy.SalePrice, true) => filtered.OrderBy(p => p.SalePrice),
            (ProductSortBy.SalePrice, false) => filtered.OrderByDescending(p => p.SalePrice),

            (ProductSortBy.ImportPrice, true) => filtered.OrderBy(p => p.ImportPrice),
            (ProductSortBy.ImportPrice, false) => filtered.OrderByDescending(p => p.ImportPrice),

            (ProductSortBy.StockQuantity, true) => filtered.OrderBy(p => p.StockQuantity),
            (ProductSortBy.StockQuantity, false) => filtered.OrderByDescending(p => p.StockQuantity),

            (ProductSortBy.CreatedAt, true) => filtered.OrderBy(p => p.ProductId),      // nếu bạn không có CreatedAt trong select
            (ProductSortBy.CreatedAt, false) => filtered.OrderByDescending(p => p.ProductId),

            (_, true) => filtered.OrderBy(p => p.Name),
            (_, false) => filtered.OrderByDescending(p => p.Name),
        };

        var total2 = filtered.Count();

        var items2 = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                Name = p.Name,
                ImportPrice = p.ImportPrice,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.CategoryName
            })
            .ToList();

        var now2 = DateTime.UtcNow;
        var ids2 = items2.Select(x => x.ProductId).ToList();
        var discountMap2 = await GetBestDiscountMapAsync(ids2, now2, ct);

        foreach (var p in items2)
        {
            var pct = discountMap2.TryGetValue(p.ProductId, out var best) ? best : 0;
            p.DiscountPct = pct;
            p.FinalPrice = PriceCalc.ApplyDiscount(p.SalePrice, pct);
        }


        return new PagedResult<ProductListItemDto>
        {
            Items = items2,
            TotalItems = total2,
            Page = page,
            PageSize = pageSize
        };
    }


    public async Task<ProductDetailDto?> GetProductByIdAsync(int productId, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.ProductId == productId, ct);

        if (product == null)
            return null;


        var now = DateTime.UtcNow;
        var discountMap = await GetBestDiscountMapAsync(new[] { product.ProductId }, now, ct);
        var pct = discountMap.TryGetValue(product.ProductId, out var best) ? best : 0;
        var final = MyShopServer.Application.Common.PriceCalc.ApplyDiscount(product.SalePrice, pct);

        return new ProductDetailDto
        {
            ProductId = product.ProductId,
            Sku = product.Sku,
            Name = product.Name,
            ImportPrice = product.ImportPrice,
            SalePrice = product.SalePrice,
            DiscountPct = pct,
            FinalPrice = final,
            StockQuantity = product.StockQuantity,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            ImagePaths = product.Images.Select(i => i.ImagePath).ToList()
        };

    }

    public async Task<ProductDetailDto> CreateProductAsync(
    ProductDetailDto dto,
    CancellationToken ct = default)
    {
        // ====== VALIDATION TRƯỚC KHI SAVE ======

        // 1. Category phải tồn tại
        var categoryExists = await _db.Categories
            .AnyAsync(c => c.CategoryId == dto.CategoryId, ct);

        if (!categoryExists)
        {
            throw new Exception($"Category with id {dto.CategoryId} does not exist.");
        }

        // 2. SKU không được trùng
        var skuExists = await _db.Products
            .AnyAsync(p => p.Sku == dto.Sku, ct);

        if (skuExists)
        {
            throw new Exception($"SKU '{dto.Sku}' already exists.");
        }

        // ====== TẠO ENTITY ======
        var entity = new Domain.Entities.Product
        {
            Sku = dto.Sku,
            Name = dto.Name,
            ImportPrice = dto.ImportPrice,
            SalePrice = dto.SalePrice,
            StockQuantity = dto.StockQuantity,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.ImagePaths?.Any() == true)
        {
            entity.Images = dto.ImagePaths
                .Select(path => new Domain.Entities.ProductImage
                {
                    ImagePath = path
                })
                .ToList();
        }

        try
        {
            _db.Products.Add(entity);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException dbEx)
        {
            // Bọc inner exception cho message dễ hiểu hơn
            var message = dbEx.InnerException?.Message ?? dbEx.Message;
            throw new Exception($"Failed to create product: {message}");
        }

        // Load navigation
        await _db.Entry(entity).Reference(p => p.Category).LoadAsync(ct);
        await _db.Entry(entity).Collection(p => p.Images).LoadAsync(ct);

        return new ProductDetailDto
        {
            ProductId = entity.ProductId,
            Sku = entity.Sku,
            Name = entity.Name,
            ImportPrice = entity.ImportPrice,
            SalePrice = entity.SalePrice,
            StockQuantity = entity.StockQuantity,
            Description = entity.Description,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty,
            ImagePaths = entity.Images.Select(i => i.ImagePath).ToList()
        };
    }

    public async Task<ProductDetailDto> UpdateProductAsync(int productId, ProductDetailDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Products
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.ProductId == productId, ct);

        if (entity == null)
            throw new Exception("Product not found");

        entity.Sku = dto.Sku;
        entity.Name = dto.Name;
        entity.ImportPrice = dto.ImportPrice;
        entity.SalePrice = dto.SalePrice;
        entity.StockQuantity = dto.StockQuantity;
        entity.Description = dto.Description;
        entity.CategoryId = dto.CategoryId;

        // cập nhật images: xoá hết cũ, thêm mới (simple)
        if (dto.ImagePaths != null)
        {
            _db.ProductImages.RemoveRange(entity.Images);
            entity.Images = dto.ImagePaths
                .Select(path => new Domain.Entities.ProductImage
                {
                    ProductId = entity.ProductId,
                    ImagePath = path
                })
                .ToList();
        }

        await _db.SaveChangesAsync(ct);

        await _db.Entry(entity).Reference(p => p.Category).LoadAsync(ct);
        await _db.Entry(entity).Collection(p => p.Images).LoadAsync(ct);

        return new ProductDetailDto
        {
            ProductId = entity.ProductId,
            Sku = entity.Sku,
            Name = entity.Name,
            ImportPrice = entity.ImportPrice,
            SalePrice = entity.SalePrice,
            StockQuantity = entity.StockQuantity,
            Description = entity.Description,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty,
            ImagePaths = entity.Images.Select(i => i.ImagePath).ToList()
        };
    }

    public async Task<bool> DeleteProductAsync(int productId, CancellationToken ct = default)
    {
        // Nếu sản phẩm đã có trong OrderItem → không cho xóa
        var hasOrderItems = await _db.OrderItems
            .AnyAsync(oi => oi.ProductId == productId, ct);

        if (hasOrderItems)
        {
            // bạn có thể ném Exception để mutation trả message đẹp hơn
            throw new Exception("Cannot delete product because it has related order items.");
        }

        var entity = await _db.Products
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.ProductId == productId, ct);

        if (entity == null)
            return false;

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

}
