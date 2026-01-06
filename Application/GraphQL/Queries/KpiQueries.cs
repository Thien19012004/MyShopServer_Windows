using HotChocolate.Authorization;
using HotChocolate.Types;
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
        int page,
        int pageSize,
    [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
      {
   var result = await kpiService.GetKpiTiersAsync(page, pageSize, ct);
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
int? saleId,
        int? year,
      int? month,
    int page,
        int pageSize,
        [Service] IKpiService kpiService,
    CancellationToken ct)
    {
        try
        {
   var result = await kpiService.GetSaleKpiTargetsAsync(saleId, year, month, page, pageSize, ct);
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
      int? saleId,
        int? year,
        int? month,
  int page,
      int pageSize,
        [Service] IKpiService kpiService,
        CancellationToken ct)
    {
        try
        {
            var result = await kpiService.GetKpiCommissionsAsync(saleId, year, month, page, pageSize, ct);
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

    // ===== KPI DASHBOARD (Sale xem dashboard c?a mình) =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator), nameof(RoleName.Sale) })]
    public async Task<KpiDashboardResultDto> KpiDashboard(
     int saleId,
        int? year,
        int? month,
        [Service] IKpiService kpiService,
        CancellationToken ct)
 {
     try
   {
   var result = await kpiService.GetKpiDashboardAsync(saleId, year, month, ct);
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
