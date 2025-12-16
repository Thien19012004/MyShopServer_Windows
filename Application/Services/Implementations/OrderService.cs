using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Orders;
using MyShopServer.Domain.Enums;
using MyShopServer.Infrastructure.Data;
using MyShopServer.Application.Common;

namespace MyShopServer.Application.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IPromotionService _promotion;

    public OrderService(AppDbContext db, IPromotionService promotion)
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

    private async Task EnsurePromotionStillValidWhenPayAsync(
    IReadOnlyCollection<Domain.Entities.OrderItem> items,
    DateTime at,
    CancellationToken ct)
    {
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();

        // lấy salePrice hiện tại của product
        var priceMap = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .Select(p => new { p.ProductId, p.SalePrice })
            .ToDictionaryAsync(x => x.ProductId, x => x.SalePrice, ct);

        // best discount hiện tại (active tại thời điểm at)
        var discountMap = await GetBestDiscountMapAsync(productIds, at, ct);

        foreach (var item in items)
        {
            if (!priceMap.TryGetValue(item.ProductId, out var salePriceNow))
                throw new Exception($"Product {item.ProductId} not found.");

            // suy ra: order này "đang dùng promotion" nếu unitPrice thấp hơn salePrice hiện tại
            var hadDiscount = item.UnitPrice < salePriceNow;
            if (!hadDiscount) continue;

            var bestPct = discountMap.TryGetValue(item.ProductId, out var best) ? best : 0;
            if (bestPct <= 0)
                throw new Exception($"Promotion expired for productId={item.ProductId}. Cannot pay.");

            var expectedUnit = MyShopServer.Application.Common.PriceCalc.ApplyDiscount(salePriceNow, bestPct);

            if (expectedUnit != item.UnitPrice)
                throw new Exception(
                    $"Promotion changed for productId={item.ProductId}. " +
                    $"Expected unitPrice={expectedUnit} but order has unitPrice={item.UnitPrice}. Cannot pay.");
        }
    }


    // ==========================
    // CREATE
    // ==========================
    public async Task<OrderDetailDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new Exception("Order must have at least one item.");
        }

        // Kiểm tra sale
        var sale = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserId == dto.SaleId, ct);

        if (sale == null)
        {
            throw new Exception($"Sale with id {dto.SaleId} does not exist.");
        }

        // Load các product liên quan
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync(ct);

        var at = DateTime.UtcNow;
        var discountMap = await GetBestDiscountMapAsync(productIds, at, ct);

        if (products.Count != productIds.Count)
        {
            throw new Exception("Some products in the order do not exist.");
        }

        // Tạo Order + Items
        var order = new Domain.Entities.Order
        {
            CustomerId = dto.CustomerId,
            SaleId = dto.SaleId,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        int total = 0;
        var items = new List<Domain.Entities.OrderItem>();

        foreach (var i in dto.Items)
        {
            var product = products.Single(p => p.ProductId == i.ProductId);
            var pct = discountMap.TryGetValue(product.ProductId, out var best) ? best : 0;
            var unitPrice = PriceCalc.ApplyDiscount(product.SalePrice, pct);
            var lineTotal = unitPrice * i.Quantity;

            total += lineTotal;

            items.Add(new Domain.Entities.OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = lineTotal
            });
        }

        order.TotalPrice = total;
        order.Items = items;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return await GetOrderDetailInternalAsync(order.OrderId, ct)
            ?? throw new Exception("Failed to load created order.");
    }

    // ==========================
    // UPDATE
    // ==========================
    public async Task<OrderDetailDto> UpdateOrderAsync(int orderId, UpdateOrderDto dto, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Include(o => o.Sale)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);


        if (order == null)
            throw new Exception("Order not found");

        // Nếu đổi sang Paid: chỉ cho phép từ Created và phải check promotion còn hiệu lực (nếu order đang hưởng giảm)
        if (dto.Status.HasValue && dto.Status.Value == OrderStatus.Paid)
        {
            if (order.Status != OrderStatus.Created)
                throw new Exception("Only Created orders can be paid.");

            // không cho sửa items trong lúc pay (optional nhưng hợp lý)
            // nếu bạn vẫn cho sửa items, thì kiểm tra sau khi replace items
            var at = DateTime.UtcNow;
            await EnsurePromotionStillValidWhenPayAsync(order.Items.ToList(), at, ct);
        }


        // Cập nhật customer
        if (dto.CustomerId.HasValue)
        {
            // hoặc check customer có tồn tại không nếu cần
            order.CustomerId = dto.CustomerId.Value;
        }

        // Cập nhật status
        if (dto.Status.HasValue)
        {
            order.Status = dto.Status.Value;
        }

        // Nếu có Items mới → replace toàn bộ
        if (dto.Items != null)
        {
            if (dto.Items.Count == 0)
                throw new Exception("Order must have at least one item.");

            // Xoá items cũ
            _db.OrderItems.RemoveRange(order.Items);
            order.Items.Clear();

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _db.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync(ct);

            var at = DateTime.UtcNow;
            var discountMap = await GetBestDiscountMapAsync(productIds, at, ct);

            if (products.Count != productIds.Count)
                throw new Exception("Some products in the order do not exist.");

            int total = 0;
            var newItems = new List<Domain.Entities.OrderItem>();

            foreach (var i in dto.Items)
            {
                var product = products.Single(p => p.ProductId == i.ProductId);
                var pct = discountMap.TryGetValue(product.ProductId, out var best) ? best : 0;
                var unitPrice = PriceCalc.ApplyDiscount(product.SalePrice, pct);
                var lineTotal = unitPrice * i.Quantity;
                total += lineTotal;

                newItems.Add(new Domain.Entities.OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = lineTotal
                });
            }

            order.TotalPrice = total;
            order.Items = newItems;
        }

        await _db.SaveChangesAsync(ct);

        return await GetOrderDetailInternalAsync(order.OrderId, ct)
            ?? throw new Exception("Failed to load updated order.");
    }

    // ==========================
    // DELETE
    // ==========================
    public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

        if (order == null)
            return false;

        // Nếu muốn chặn xoá đơn đã Paid:
        // if (order.Status == OrderStatus.Paid)
        //     throw new Exception("Cannot delete a paid order.");

        _db.OrderItems.RemoveRange(order.Items);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ==========================
    // GET BY ID
    // ==========================
    public async Task<OrderDetailDto?> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
    {
        return await GetOrderDetailInternalAsync(orderId, ct);
    }

    private async Task<OrderDetailDto?> GetOrderDetailInternalAsync(int orderId, CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Customer)
            .Include(o => o.Sale)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

        if (order == null) return null;

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
                .ToList()
        };
    }

    // ==========================
    // LIST + FILTER + PAGING
    // ==========================
    public async Task<PagedResult<OrderListItemDto>> GetOrdersAsync(
        OrderQueryOptions options,
        CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Sale)
            .Include(o => o.Items)
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
        {
            query = query.Where(o => o.CustomerId == options.CustomerId);
        }

        if (options.SaleId.HasValue)
        {
            query = query.Where(o => o.SaleId == options.SaleId);
        }

        if (options.Status.HasValue)
        {
            query = query.Where(o => o.Status == options.Status);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalItems = await query.CountAsync(ct);

        var page = options.Page <= 0 ? 1 : options.Page;
        var pageSize = options.PageSize <= 0 ? 10 : options.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListItemDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer!.Name,
                SaleName = o.Sale!.FullName,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                ItemsCount = o.Items.Count
            })
            .ToListAsync(ct);

        return new PagedResult<OrderListItemDto>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }
}
