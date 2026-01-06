using HotChocolate.Authorization;
using MyShopServer.Application.GraphQL.Inputs.Kpi;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Kpi;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class KpiMutations
{
    // ===== KPI TIER MANAGEMENT (Admin only) =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
    public async Task<KpiTierResultDto> CreateKpiTier(
        CreateKpiTierInput input,
        [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var dto = new KpiTierDto
            {
                Name = input.Name,
                MinRevenue = input.MinRevenue,
                BonusPercent = input.BonusPercent,
                Description = input.Description,
                DisplayOrder = input.DisplayOrder
            };

            var result = await kpiService.CreateKpiTierAsync(dto, ct);
            return new KpiTierResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "KPI tier created successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new KpiTierResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
    public async Task<KpiTierResultDto> UpdateKpiTier(
        int kpiTierId,
 UpdateKpiTierInput input,
        [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            // Load existing to support partial updates
            var existing = await kpiService.GetKpiTierByIdAsync(kpiTierId, ct);
            if (existing == null)
            {
                return new KpiTierResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "KPI tier not found",
                    Data = null
                };
            }

            var dto = new KpiTierDto
            {
                KpiTierId = kpiTierId,
                Name = input.Name ?? existing.Name,
                MinRevenue = input.MinRevenue ?? existing.MinRevenue,
                BonusPercent = input.BonusPercent ?? existing.BonusPercent,
                Description = input.Description ?? existing.Description,
                DisplayOrder = input.DisplayOrder ?? existing.DisplayOrder
            };

            var result = await kpiService.UpdateKpiTierAsync(kpiTierId, dto, ct);
            if (result == null)
            {
                return new KpiTierResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "KPI tier not found",
                    Data = null
                };
            }

            return new KpiTierResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "KPI tier updated successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new KpiTierResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
    public async Task<KpiTierResultDto> DeleteKpiTier(
   int kpiTierId,
   [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var ok = await kpiService.DeleteKpiTierAsync(kpiTierId, ct);
            if (!ok)
            {
                return new KpiTierResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "KPI tier not found",
                    Data = null
                };
            }

            return new KpiTierResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "KPI tier deleted successfully",
                Data = null
            };
        }
        catch (Exception ex)
        {
            return new KpiTierResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // ===== SET MONTHLY TARGET (Admin/Moderator) =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<SaleKpiTargetResultDto> SetMonthlyTarget(
    SetMonthlyTargetInput input,
    [Service] IKpiService kpiService,
    CancellationToken ct)
    {
        try
        {
            var dto = new SetMonthlyTargetDto
            {
                SaleId = input.SaleId,
                Year = input.Year,
                Month = input.Month,
                TargetRevenue = input.TargetRevenue
            };

            var result = await kpiService.SetMonthlyTargetAsync(dto, ct);
            return new SaleKpiTargetResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Monthly target set successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new SaleKpiTargetResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // ===== CALCULATE MONTHLY KPI (Admin only) =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
    public async Task<KpiCalculateResultDto> CalculateMonthlyKpi(
    CalculateMonthlyKpiInput input,
    [Service] IKpiService kpiService,
    CancellationToken ct)
    {
        try
        {
            var dto = new CalculateMonthlyKpiDto
            {
                Year = input.Year,
                Month = input.Month,
                SaleId = input.SaleId
            };

            var results = await kpiService.CalculateMonthlyKpiAsync(dto, ct);
            return new KpiCalculateResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = $"KPI calculated for {results.Count} sale(s)",
                Data = results
            };
        }
        catch (Exception ex)
        {
            return new KpiCalculateResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
