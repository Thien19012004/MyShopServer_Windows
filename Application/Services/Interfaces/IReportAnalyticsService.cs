using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Reports;

namespace MyShopServer.Application.Services.Interfaces;

public interface IReportAnalyticsService
{
    Task<List<ProductSalesSeriesDto>> GetProductSalesSeriesAsync(
        DateTime from,
        DateTime to,
        ReportGroupBy groupBy,
        int? categoryId,
        int top,
        CancellationToken ct = default);

    Task<List<RevenueProfitPointDto>> GetRevenueProfitSeriesAsync(
        DateTime from,
        DateTime to,
        ReportGroupBy groupBy,
        CancellationToken ct = default);
}
