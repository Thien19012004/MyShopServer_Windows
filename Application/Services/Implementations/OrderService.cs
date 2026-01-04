using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Orders;
using MyShopServer.Domain.Enums;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    // =========================================================
    // PROMOTION HELPERS
    // =========================================================

    // Only allow Order-scope promotions for OrderPromotionIds
    private async Task<List<int>> NormalizeAndValidateOrderPromotionIdsAsync(
        List<int>? promotionIds,
        DateTime at,
        CancellationToken ct)
    {
        var ids = (promotionIds ?? new List<int>())
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0) return new List<int>();

        var promos = await _db.Promotions
            .AsNoTracking()
            .Where(p => ids.Contains(p.PromotionId))
            .Select(p => new
            {
                p.PromotionId,
                p.Scope,
                p.StartDate,
                p.EndDate,
                p.Name,
                p.DiscountPercent
            })
            .ToListAsync(ct);

        if (promos.Count != ids.Count)
            throw new Exception("Some promotionIds do not exist.");

        if (promos.Any(p => p.Scope != PromotionScope.Order))
            throw new Exception("Only Order-scope promotions are allowed for order.");

        var notActive = promos.FirstOrDefault(p => !(p.StartDate <= at && at <= p.EndDate));
        if (notActive != null)
            throw new Exception($"Promotion '{notActive.Name}' is not active.");

        if (promos.Any(p => p.DiscountPercent < 0 || p.DiscountPercent > 100))
            throw new Exception("Invalid DiscountPercent (must be 0..100).");

        return ids;
    }

    // Best discount (max) for each product from:
    // - product promotions (scope Product)
    // - category promotions (scope Category)
    private async Task<Dictionary<int, int>> GetBestProductOrCategoryDiscountMapAsync(
        IReadOnlyCollection<int> productIds,
        DateTime at,
        CancellationToken ct)
    {
        var map = productIds.ToDictionary(id => id, _ => 0);
        if (productIds.Count == 0) return map;

        // Product-scope best
        var productBest = await _db.ProductPromotions
            .AsNoTracking()
            .Where(pp => productIds.Contains(pp.ProductId))
            .Where(pp =>
                pp.Promotion.Scope == PromotionScope.Product &&
                pp.Promotion.StartDate <= at && at <= pp.Promotion.EndDate)
            .GroupBy(pp => pp.ProductId)
            .Select(g => new { ProductId = g.Key, Best = g.Max(x => x.Promotion.DiscountPercent) })
            .ToListAsync(ct);

        foreach (var x in productBest)
            map[x.ProductId] = Math.Max(map[x.ProductId], Math.Clamp(x.Best, 0, 100));

        // Category-scope best
        var categoryBest = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .Select(p => new { p.ProductId, p.CategoryId })
            .Join(
                _db.CategoryPromotions.AsNoTracking(),
                pc => pc.CategoryId,
                cp => cp.CategoryId,
                (pc, cp) => new { pc.ProductId, Promo = cp.Promotion }
            )
            .Where(x =>
                x.Promo.Scope == PromotionScope.Category &&
                x.Promo.StartDate <= at && at <= x.Promo.EndDate)
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Best = g.Max(x => x.Promo.DiscountPercent) })
            .ToListAsync(ct);

        foreach (var x in categoryBest)
            map[x.ProductId] = Math.Max(map[x.ProductId], Math.Clamp(x.Best, 0, 100));

        return map;
    }

    // AUTO apply product/category promotions to compute unit price
    private async Task<Dictionary<int, int>> CalculateUnitPriceMap_AutoProductCategoryPromoAsync(
        IReadOnlyCollection<int> productIds,
        DateTime at,
        CancellationToken ct)
    {
        if (productIds.Count == 0) return new Dictionary<int, int>();

        var basePrices = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .Select(p => new { p.ProductId, p.SalePrice })
            .ToListAsync(ct);

        if (basePrices.Count != productIds.Count)
            throw new Exception("Some products in the order do not exist.");

        var discountMap = await GetBestProductOrCategoryDiscountMapAsync(productIds, at, ct);

        return basePrices.ToDictionary(
            x => x.ProductId,
            x =>
            {
                var pct = discountMap.TryGetValue(x.ProductId, out var d) ? d : 0;
                return PriceCalc.ApplyDiscount(x.SalePrice, pct);
            }
        );
    }

    // discountPercentApplied = best order promo percent (max)
    // discountAmount uses LONG to avoid overflow
    private async Task<(int pct, int amount)> CalculateOrderDiscountAsync(
        int subtotal,
        List<int> orderPromotionIds,
        DateTime at,
        CancellationToken ct)
    {
        if (subtotal <= 0) return (0, 0);
        if (orderPromotionIds == null || orderPromotionIds.Count == 0) return (0, 0);

        var best = await _db.Promotions
            .AsNoTracking()
            .Where(p => orderPromotionIds.Contains(p.PromotionId))
            .Where(p => p.Scope == PromotionScope.Order)
            .Where(p => p.StartDate <= at && at <= p.EndDate)
            .Select(p => (int?)p.DiscountPercent)
            .MaxAsync(ct);

        var pct = Math.Clamp(best ?? 0, 0, 100);
        if (pct <= 0) return (0, 0);

        // ✅ IMPORTANT: use long to avoid overflow
        long amountLong = (long)subtotal * pct / 100;
        if (amountLong < 0) amountLong = 0;
        if (amountLong > subtotal) amountLong = subtotal;

        return (pct, (int)amountLong);
    }

    // =========================================================
    // CREATE
    // =========================================================
    public async Task<OrderDetailDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new Exception("Order must have at least one item.");

        var saleExists = await _db.Users.AsNoTracking()
            .AnyAsync(u => u.UserId == dto.SaleId, ct);
        if (!saleExists)
            throw new Exception($"Sale with id {dto.SaleId} does not exist.");

        var at = DateTime.UtcNow;

        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _db.Customers.AsNoTracking()
                .AnyAsync(c => c.CustomerId == dto.CustomerId.Value, ct);
            if (!customerExists)
                throw new Exception($"Customer with id {dto.CustomerId.Value} does not exist.");
        }

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var unitPriceMap = await CalculateUnitPriceMap_AutoProductCategoryPromoAsync(productIds, at, ct);

        // order-scope promotions only
        var orderPromoIds = await NormalizeAndValidateOrderPromotionIdsAsync(dto.PromotionIds, at, ct);

        var order = new Domain.Entities.Order
        {
            CustomerId = dto.CustomerId,
            SaleId = dto.SaleId,
            Status = OrderStatus.Created,
            CreatedAt = at
        };

        var items = new List<Domain.Entities.OrderItem>();
        long subtotalLong = 0;

        foreach (var i in dto.Items)
        {
            if (i.Quantity <= 0)
                throw new Exception("Quantity must be greater than 0.");

            var unit = unitPriceMap[i.ProductId];

            // ✅ IMPORTANT: use long
            long lineLong = (long)unit * i.Quantity;
            if (lineLong > int.MaxValue)
                throw new OverflowException("Line total exceeds int.MaxValue. Consider using long for money.");

            subtotalLong += lineLong;
            if (subtotalLong > int.MaxValue)
                throw new OverflowException("Subtotal exceeds int.MaxValue. Consider using long for money.");

            items.Add(new Domain.Entities.OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = unit,
                TotalPrice = (int)lineLong
            });
        }

        var subtotal = (int)subtotalLong;

        var (pct, discountAmount) = await CalculateOrderDiscountAsync(subtotal, orderPromoIds, at, ct);

        order.Items = items;
        order.TotalPrice = subtotal - discountAmount;

        if (orderPromoIds.Count > 0)
        {
            order.OrderPromotions = orderPromoIds
                .Select(pid => new Domain.Entities.OrderPromotion { PromotionId = pid })
                .ToList();
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return await GetOrderDetailInternalAsync(order.OrderId, ct)
               ?? throw new Exception("Failed to load created order.");
    }

    // =========================================================
    // UPDATE
    // =========================================================
    public async Task<OrderDetailDto> UpdateOrderAsync(int orderId, UpdateOrderDto dto, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.OrderPromotions)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

        if (order == null)
            throw new Exception("Order not found");

        // Paid orders cannot be updated
        if (order.Status == OrderStatus.Paid)
            throw new Exception("Paid orders cannot be updated.");

        var at = DateTime.UtcNow;

        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _db.Customers.AsNoTracking()
                .AnyAsync(c => c.CustomerId == dto.CustomerId.Value, ct);
            if (!customerExists)
                throw new Exception($"Customer with id {dto.CustomerId.Value} does not exist.");

            order.CustomerId = dto.CustomerId.Value;
        }

        if (dto.Status.HasValue)
            order.Status = dto.Status.Value;

        // ---- Update order promotions (Order scope only) ----
        var promotionsChanged = false;
        if (dto.PromotionIds != null)
        {
            var newIds = await NormalizeAndValidateOrderPromotionIdsAsync(dto.PromotionIds, at, ct);

            var oldIds = order.OrderPromotions
                .Select(x => x.PromotionId)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var sortedNew = newIds.OrderBy(x => x).ToList();

            if (!oldIds.SequenceEqual(sortedNew))
            {
                promotionsChanged = true;

                _db.OrderPromotions.RemoveRange(order.OrderPromotions);
                order.OrderPromotions.Clear();

                foreach (var pid in sortedNew)
                {
                    order.OrderPromotions.Add(new Domain.Entities.OrderPromotion
                    {
                        OrderId = order.OrderId,
                        PromotionId = pid
                    });
                }
            }
        }

        // ---- Replace items ----
        if (dto.Items != null)
        {
            if (dto.Items.Count == 0)
                throw new Exception("Order must have at least one item.");

            _db.OrderItems.RemoveRange(order.Items);
            order.Items.Clear();

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var unitPriceMap = await CalculateUnitPriceMap_AutoProductCategoryPromoAsync(productIds, at, ct);

            var newItems = new List<Domain.Entities.OrderItem>();
            long subtotalLong = 0;

            foreach (var i in dto.Items)
            {
                if (i.Quantity <= 0)
                    throw new Exception("Quantity must be greater than 0.");

                var unit = unitPriceMap[i.ProductId];

                long lineLong = (long)unit * i.Quantity;
                if (lineLong > int.MaxValue)
                    throw new OverflowException("Line total exceeds int.MaxValue. Consider using long for money.");

                subtotalLong += lineLong;
                if (subtotalLong > int.MaxValue)
                    throw new OverflowException("Subtotal exceeds int.MaxValue. Consider using long for money.");

                newItems.Add(new Domain.Entities.OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = unit,
                    TotalPrice = (int)lineLong
                });
            }

            order.Items = newItems;

            var subtotal = (int)subtotalLong;
            var orderPromoIds = order.OrderPromotions.Select(x => x.PromotionId).Distinct().ToList();
            var (_, discount) = await CalculateOrderDiscountAsync(subtotal, orderPromoIds, at, ct);

            order.TotalPrice = subtotal - discount;
        }
        else if (promotionsChanged)
        {
            // Only promotions changed => do not recalc unit prices, only recalc order total
            var subtotal = order.Items.Sum(i => i.TotalPrice);
            var orderPromoIds = order.OrderPromotions.Select(x => x.PromotionId).Distinct().ToList();
            var (_, discount) = await CalculateOrderDiscountAsync(subtotal, orderPromoIds, at, ct);

            order.TotalPrice = subtotal - discount;
        }

        await _db.SaveChangesAsync(ct);

        return await GetOrderDetailInternalAsync(order.OrderId, ct)
               ?? throw new Exception("Failed to load updated order.");
    }

    // =========================================================
    // DELETE
    // =========================================================
    public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.OrderPromotions)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

        if (order == null) return false;

        _db.OrderItems.RemoveRange(order.Items);
        _db.OrderPromotions.RemoveRange(order.OrderPromotions);
        _db.Orders.Remove(order);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    // =========================================================
    // GET BY ID
    // =========================================================
    public async Task<OrderDetailDto?> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
        => await GetOrderDetailInternalAsync(orderId, ct);

    private async Task<OrderDetailDto?> GetOrderDetailInternalAsync(int orderId, CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer)
            .Include(o => o.Sale)
            .Include(o => o.OrderPromotions)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

        if (order == null) return null;

        var subtotal = order.Items.Sum(i => i.TotalPrice);

        // Make these fields ALWAYS consistent with stored TotalPrice
        var discountAmount = Math.Clamp(subtotal - order.TotalPrice, 0, subtotal);

        // percent derived from actual stored discount (rounded)
        var discountPercent = subtotal <= 0
     ? 0
        : (int)Math.Round(discountAmount * 100.0 / subtotal);

        var at = order.CreatedAt;

        // =====================
        // PROMOTION IDS (BEST PER SCOPE)
        // =====================
        // Goal: return promotion IDs such that:
        // - ORDER scope: only best order promotion (max pct) among attached OrderPromotions (if any)
        // - PRODUCT scope: best per product (because each product can have different best product promo)
        // - CATEGORY scope: best per category involved in the order (max pct)
        // This avoids returning multiple category promotions when only the best one actually applies.

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

        // --- Order-scope: pick ONLY best among order.OrderPromotions ---
        List<int> bestOrderPromoIds = new();
        if (order.OrderPromotions.Count > 0)
        {
            var attachedOrderPromoIds = order.OrderPromotions
                .Select(op => op.PromotionId)
                .Distinct()
                .ToList();

            var bestOrderPromoId = await _db.Promotions
                .AsNoTracking()
                .Where(p => attachedOrderPromoIds.Contains(p.PromotionId))
                .Where(p => p.Scope == PromotionScope.Order)
                .Where(p => p.StartDate <= at && at <= p.EndDate)
                .OrderByDescending(p => p.DiscountPercent)
                .ThenBy(p => p.PromotionId) // deterministic
                .Select(p => (int?)p.PromotionId)
                .FirstOrDefaultAsync(ct);

            if (bestOrderPromoId.HasValue)
                bestOrderPromoIds.Add(bestOrderPromoId.Value);
        }

        // Load product -> category
        var productCategoryMap = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .Select(p => new { p.ProductId, p.CategoryId })
            .ToListAsync(ct);

        var categoryByProduct = productCategoryMap.ToDictionary(x => x.ProductId, x => x.CategoryId);
        var categoryIds = productCategoryMap.Select(x => x.CategoryId).Distinct().ToList();

        // Active product promos for these products
        var activeProductPromos = await _db.ProductPromotions
            .AsNoTracking()
            .Where(pp => productIds.Contains(pp.ProductId))
            .Where(pp =>
                pp.Promotion.Scope == PromotionScope.Product &&
                pp.Promotion.StartDate <= at && at <= pp.Promotion.EndDate)
            .Select(pp => new { pp.ProductId, pp.PromotionId, pp.Promotion.DiscountPercent })
            .ToListAsync(ct);

        // Active category promos for these categories
        var activeCategoryPromos = await _db.CategoryPromotions
            .AsNoTracking()
            .Where(cp => categoryIds.Contains(cp.CategoryId))
            .Where(cp =>
                cp.Promotion.Scope == PromotionScope.Category &&
                cp.Promotion.StartDate <= at && at <= cp.Promotion.EndDate)
            .Select(cp => new { cp.CategoryId, cp.PromotionId, cp.Promotion.DiscountPercent })
            .ToListAsync(ct);

        // --- Product-scope: best promo per product (used for that product) ---
        var bestProductPromoIds = new HashSet<int>();
        foreach (var pid in productIds)
        {
            var best = activeProductPromos
                .Where(x => x.ProductId == pid)
                .Select(x => new { x.PromotionId, Pct = Math.Clamp(x.DiscountPercent, 0, 100) })
                .Where(x => x.Pct > 0)
                .OrderByDescending(x => x.Pct)
                .ThenBy(x => x.PromotionId)
                .FirstOrDefault();

            if (best != null)
                bestProductPromoIds.Add(best.PromotionId);
        }

        // --- Category-scope: best promo per category (NOT union across all categories) ---
        var bestCategoryPromoIds = new HashSet<int>();
        foreach (var cid in categoryIds)
        {
            var best = activeCategoryPromos
                .Where(x => x.CategoryId == cid)
                .Select(x => new { x.PromotionId, Pct = Math.Clamp(x.DiscountPercent, 0, 100) })
                .Where(x => x.Pct > 0)
                .OrderByDescending(x => x.Pct)
                .ThenBy(x => x.PromotionId)
                .FirstOrDefault();

            if (best != null)
                bestCategoryPromoIds.Add(best.PromotionId);
        }

        // Merge best IDs from each scope
        var promotionIds = bestOrderPromoIds
            .Concat(bestProductPromoIds)
            .Concat(bestCategoryPromoIds)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return new OrderDetailDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name,
            CustomerPhone = order.Customer?.Phone,
            SaleId = order.SaleId,
            SaleName = order.Sale?.FullName,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt,
            Items = order.Items
            .OrderBy(i => i.OrderItemId)
            .Select(i => new OrderItemDto
            {
                OrderItemId = i.OrderItemId,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            })
            .ToList(),

            PromotionIds = promotionIds,
            Subtotal = subtotal,
            OrderDiscountAmount = discountAmount,
            OrderDiscountPercentApplied = discountPercent
        };
    }

    // =========================================================
    // LIST + FILTER + PAGING
    // =========================================================
    public async Task<PagedResult<OrderListItemDto>> GetOrdersAsync(OrderQueryOptions options, CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Sale)
            .Include(o => o.Items)
            .Include(o => o.OrderPromotions)
            .AsQueryable();

        if (options.FromDate.HasValue)
        {
            var from = options.FromDate.Value.Date;
            query = query.Where(o => o.CreatedAt >= from);
        }

        if (options.ToDate.HasValue)
        {
            var to = options.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.CreatedAt <= to);
        }

        if (options.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == options.CustomerId.Value);

        if (options.SaleId.HasValue)
            query = query.Where(o => o.SaleId == options.SaleId.Value);

        if (options.Status.HasValue)
            query = query.Where(o => o.Status == options.Status.Value);

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalItems = await query.CountAsync(ct);

        var page = options.Page <= 0 ? 1 : options.Page;
        var pageSize = options.PageSize <= 0 ? 10 : options.PageSize;

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = orders.Select(o =>
        {
            var subtotal = o.Items.Sum(x => x.TotalPrice);
            var discount = Math.Clamp(subtotal - o.TotalPrice, 0, subtotal);

            return new OrderListItemDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer?.Name,
                SaleName = o.Sale?.FullName,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                ItemsCount = o.Items.Count,
                Subtotal = subtotal,
                OrderDiscountAmount = discount
            };
        }).ToList();

        return new PagedResult<OrderListItemDto>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }
}
