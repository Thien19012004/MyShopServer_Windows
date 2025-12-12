using HotChocolate.Authorization;
using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Reports;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Reports;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ReportQueries
{

    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator)
    })]
    public async Task<ProductSalesSeriesListResultDto> ReportProductSalesSeries(
        DateRangeInput dateRange,
        ReportGroupBy groupBy,
        ProductSalesReportFilterInput? filter,
        [Service] IReportAnalyticsService service,
        CancellationToken ct = default)
    {
        try
        {
            var from = dateRange.From ?? DateTime.UtcNow.Date.AddDays(-30);
            var to = dateRange.To ?? DateTime.UtcNow.Date;

            var categoryId = filter?.CategoryId;
            var top = filter?.Top ?? 10;

            var data = await service.GetProductSalesSeriesAsync(from, to, groupBy, categoryId, top, ct);

            return new ProductSalesSeriesListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get product sales report success",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new ProductSalesSeriesListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator),
    })]
    public async Task<RevenueProfitPointListResultDto> ReportRevenueProfitSeries(
        DateRangeInput dateRange,
        ReportGroupBy groupBy,
        [Service] IReportAnalyticsService service,
        CancellationToken ct = default)
    {
        try
        {
            var from = dateRange.From ?? DateTime.UtcNow.Date.AddDays(-30);
            var to = dateRange.To ?? DateTime.UtcNow.Date;

            var data = await service.GetRevenueProfitSeriesAsync(from, to, groupBy, ct);

            return new RevenueProfitPointListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get revenue/profit report success",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new RevenueProfitPointListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
