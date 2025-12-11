using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Reports;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);

        var totalProducts = await _db.Products.CountAsync(ct);

        var todayOrdersQuery = _db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow);

        var totalOrdersToday = await todayOrdersQuery.CountAsync(ct);

        var revenueToday = await todayOrdersQuery
            .Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => (int?)o.TotalPrice, ct) ?? 0;

        return new DashboardOverviewDto
        {
            TotalProducts = totalProducts,
            TotalOrdersToday = totalOrdersToday,
            RevenueToday = revenueToday
        };
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(
        int threshold = 5,
        int take = 5,
        CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.StockQuantity < threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .Take(take)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                Name = p.Name,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync(ct);
    }

    public async Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(
        DateTime? from = null,
        DateTime? to = null,
        int take = 5,
        CancellationToken ct = default)
    {
        var query = _db.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.Status == OrderStatus.Paid);

        if (from.HasValue)
        {
            var f = from.Value.Date;
            query = query.Where(oi => oi.Order.CreatedAt >= f);
        }

        if (to.HasValue)
        {
            var t = to.Value.Date.AddDays(1);
            query = query.Where(oi => oi.Order.CreatedAt < t);
        }

        return await query
            .GroupBy(oi => new { oi.ProductId, oi.Product!.Sku, oi.Product.Name })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                Sku = g.Key.Sku,
                Name = g.Key.Name,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(
        int take = 3,
        CancellationToken ct = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Sale)
            .OrderByDescending(o => o.CreatedAt)
            .Take(take)
            .Select(o => new RecentOrderDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer != null ? o.Customer.Name : null,
                SaleName = o.Sale != null ? o.Sale.FullName : null,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<DailyRevenuePointDto>> GetDailyRevenueInMonthAsync(
        int? year = null,
        int? month = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var monthStart = new DateTime(targetYear, targetMonth, 1);
        var monthEnd = monthStart.AddMonths(1);

        return await _db.Orders
            .AsNoTracking()
            .Where(o =>
                o.Status == OrderStatus.Paid &&
                o.CreatedAt >= monthStart &&
                o.CreatedAt < monthEnd)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenuePointDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalPrice)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }
}
