using MyShopServer.DTOs.Reports;

namespace MyShopServer.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken ct = default);

    Task<List<LowStockProductDto>> GetLowStockProductsAsync(
        int threshold = 5,
        int take = 5,
        CancellationToken ct = default);

    Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(
        DateTime? from = null,
        DateTime? to = null,
        int take = 5,
        CancellationToken ct = default);

    Task<List<RecentOrderDto>> GetRecentOrdersAsync(
        int take = 3,
        CancellationToken ct = default);

    Task<List<DailyRevenuePointDto>> GetDailyRevenueInMonthAsync(
        int? year = null,
        int? month = null,
        CancellationToken ct = default);
}
