using HotChocolate.Authorization;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Kpi;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Kpi;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class KpiQueries
{
    // ===== KPI TIERS (Admin/Moderator) =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<KpiTierListResultDto> KpiTiers(
        PaginationInput? pagination,
        KpiTierFilterInput? filter,
    [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var page = pagination?.Page ?? 1;
            var pageSize = pagination?.PageSize ?? 10;

            var result = await kpiService.GetKpiTiersAsync(page, pageSize, ct);

            // Optional: filter/search in-memory (tiers set is typically small)
            if (!string.IsNullOrWhiteSpace(filter?.Search))
            {
                var term = filter.Search.Trim();
                var filtered = result.Items
                    .Where(x => (x.Name ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                result = new DTOs.Common.PagedResult<KpiTierDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = filtered.Count,
                    Items = filtered
                };
            }

            return new KpiTierListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get KPI tiers success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new KpiTierListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // ===== SALE KPI TARGETS =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator), nameof(RoleName.Sale) })]
    public async Task<SaleKpiTargetListResultDto> SaleKpiTargets(
        PaginationInput? pagination,
        SaleKpiTargetFilterInput? filter,
        [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var page = pagination?.Page ?? 1;
            var pageSize = pagination?.PageSize ?? 10;

            var result = await kpiService.GetSaleKpiTargetsAsync(filter?.SaleId, filter?.Year, filter?.Month, page, pageSize, ct);
            return new SaleKpiTargetListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get sale KPI targets success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new SaleKpiTargetListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // ===== KPI COMMISSIONS =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator), nameof(RoleName.Sale) })]
    public async Task<KpiCommissionListResultDto> KpiCommissions(
        PaginationInput? pagination,
        KpiCommissionFilterInput? filter,
        [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var page = pagination?.Page ?? 1;
            var pageSize = pagination?.PageSize ?? 10;

            var result = await kpiService.GetKpiCommissionsAsync(filter?.SaleId, filter?.Year, filter?.Month, page, pageSize, ct);
            return new KpiCommissionListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get KPI commissions success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new KpiCommissionListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // ===== KPI DASHBOARD =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator), nameof(RoleName.Sale) })]
    public async Task<KpiDashboardResultDto> KpiDashboard(
        KpiDashboardInput input,
        [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var result = await kpiService.GetKpiDashboardAsync(input.SaleId, input.Year, input.Month, ct);
            return new KpiDashboardResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get KPI dashboard success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new KpiDashboardResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
