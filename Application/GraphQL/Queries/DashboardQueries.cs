using HotChocolate.Authorization;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Reports;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class DashboardQueries
{
    // 1. Thẻ tổng quan dashboard
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<DashboardOverviewResultDto> ReportOverview(
        [Service] IDashboardService dashboardService,
        CancellationToken ct)
    {
        try
        {
            var dto = await dashboardService.GetDashboardOverviewAsync(ct);
            return new DashboardOverviewResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get overview success",
                Data = dto
            };
        }
        catch (Exception ex)
        {
            return new DashboardOverviewResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // 2. Top sản phẩm sắp hết hàng
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<LowStockProductListResultDto> ReportLowStockProducts(
        [Service] IDashboardService dashboardService,
        int threshold = 5,
        int take = 5,
        CancellationToken ct = default)
    {
        try
        {
            var list = await dashboardService.GetLowStockProductsAsync(threshold, take, ct);
            return new LowStockProductListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get low stock products success",
                Data = list
            };
        }
        catch (Exception ex)
        {
            return new LowStockProductListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // 3. Top sản phẩm bán chạy (filter theo khoảng ngày)
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<TopSellingProductListResultDto> ReportTopSellingProducts(
        [Service] IDashboardService dashboardService,
        DateRangeInput? dateRange,
        int take = 5,
        CancellationToken ct = default)
    {
        try
        {
            DateTime? from = dateRange?.From;
            DateTime? to = dateRange?.To;

            var list = await dashboardService.GetTopSellingProductsAsync(from, to, take, ct);

            return new TopSellingProductListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get top selling products success",
                Data = list
            };
        }
        catch (Exception ex)
        {
            return new TopSellingProductListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // 4. 3 đơn hàng gần nhất
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<RecentOrderListResultDto> ReportRecentOrders(
        [Service] IDashboardService dashboardService,
        int take = 3,
        CancellationToken ct = default)
    {
        try
        {
            var list = await dashboardService.GetRecentOrdersAsync(take, ct);
            return new RecentOrderListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get recent orders success",
                Data = list
            };
        }
        catch (Exception ex)
        {
            return new RecentOrderListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // 5. Biểu đồ doanh thu theo ngày trong 1 tháng (mặc định: tháng hiện tại)
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<DailyRevenueListResultDto> ReportDailyRevenueInMonth(
        int? year,
        int? month,
        [Service] IDashboardService dashboardService,
        CancellationToken ct)
    {
        try
        {
            var list = await dashboardService.GetDailyRevenueInMonthAsync(year, month, ct);
            return new DailyRevenueListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get daily revenue success",
                Data = list
            };
        }
        catch (Exception ex)
        {
            return new DailyRevenueListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
