using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.Infrastructure.Data;
using MyShopServer.DTOs.Reports;

namespace MyShopServer.Application.Services.Implementations;

public class ReportAnalyticsService : IReportAnalyticsService
{
    private readonly AppDbContext _db;

    public ReportAnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    // ---------------------------
    // Helpers: bucket start time
    // ---------------------------
    private static DateTime GetBucketStart(DateTime dt, ReportGroupBy groupBy)
    {
        dt = dt.Date;

        return groupBy switch
        {
            ReportGroupBy.Day => dt,
            ReportGroupBy.Week => StartOfWeek(dt, DayOfWeek.Monday),
            ReportGroupBy.Month => new DateTime(dt.Year, dt.Month, 1),
            ReportGroupBy.Year => new DateTime(dt.Year, 1, 1),
            _ => dt
        };
    }

    private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
    {
        // Monday-based week
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-diff).Date;
    }

    // ---------------------------
    // 1) Product quantity sold series
    // ---------------------------
    public async Task<List<ProductSalesSeriesDto>> GetProductSalesSeriesAsync(
        DateTime from,
        DateTime to,
        ReportGroupBy groupBy,
        int? categoryId,
        int top,
        CancellationToken ct = default)
    {
        // inclusive date range -> convert to [from, toExclusive)
        var fromDate = from.Date;
        var toExclusive = to.Date.AddDays(1);

        // lấy order items của đơn Paid trong range
        var baseQuery =
            from oi in _db.OrderItems.AsNoTracking()
            join o in _db.Orders.AsNoTracking() on oi.OrderId equals o.OrderId
            join p in _db.Products.AsNoTracking() on oi.ProductId equals p.ProductId
            where o.Status == OrderStatus.Paid
                  && o.CreatedAt >= fromDate
                  && o.CreatedAt < toExclusive
            select new
            {
                o.CreatedAt,
                ProductId = p.ProductId,
                p.Sku,
                p.Name,
                p.CategoryId,
                oi.Quantity
            };

        if (categoryId.HasValue)
            baseQuery = baseQuery.Where(x => x.CategoryId == categoryId.Value);

        var rows = await baseQuery.ToListAsync(ct);

        // top sản phẩm theo tổng quantity trong range
        var topProducts = rows
            .GroupBy(x => new { x.ProductId, x.Sku, x.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Sku,
                g.Key.Name,
                TotalQty = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQty)
            .Take(Math.Max(1, top))
            .ToList();

        var topIds = topProducts.Select(x => x.ProductId).ToHashSet();

        // group theo bucket (period) + product
        var grouped = rows
            .Where(x => topIds.Contains(x.ProductId))
            .GroupBy(x => new
            {
                x.ProductId,
                Bucket = GetBucketStart(x.CreatedAt, groupBy)
            })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Bucket,
                Qty = g.Sum(x => x.Quantity)
            })
            .ToList();

        // build series
        var result = new List<ProductSalesSeriesDto>();

        foreach (var p in topProducts)
        {
            var points = grouped
                .Where(x => x.ProductId == p.ProductId)
                .OrderBy(x => x.Bucket)
                .Select(x => new ReportPointDto
                {
                    Period = x.Bucket,
                    Value = x.Qty
                })
                .ToList();

            result.Add(new ProductSalesSeriesDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                Name = p.Name,
                Points = points
            });
        }

        return result;
    }

    // ---------------------------
    // 2) Revenue & Profit series
    // ---------------------------
    public async Task<List<RevenueProfitPointDto>> GetRevenueProfitSeriesAsync(
        DateTime from,
        DateTime to,
        ReportGroupBy groupBy,
        CancellationToken ct = default)
    {
        var fromDate = from.Date;
        var toExclusive = to.Date.AddDays(1);

        // lấy order items + import price để tính profit
        var rows =
            await (
                from oi in _db.OrderItems.AsNoTracking()
                join o in _db.Orders.AsNoTracking() on oi.OrderId equals o.OrderId
                join p in _db.Products.AsNoTracking() on oi.ProductId equals p.ProductId
                where o.Status == OrderStatus.Paid
                      && o.CreatedAt >= fromDate
                      && o.CreatedAt < toExclusive
                select new
                {
                    o.CreatedAt,
                    Revenue = oi.TotalPrice,              // đã có total per row
                    Cost = p.ImportPrice * oi.Quantity    // giá nhập * số lượng
                }
            ).ToListAsync(ct);

        var series = rows
            .GroupBy(x => GetBucketStart(x.CreatedAt, groupBy))
            .Select(g => new RevenueProfitPointDto
            {
                Period = g.Key,
                Revenue = g.Sum(x => x.Revenue),
                Profit = g.Sum(x => x.Revenue - x.Cost)
            })
            .OrderBy(x => x.Period)
            .ToList();

        return series;
    }
}
