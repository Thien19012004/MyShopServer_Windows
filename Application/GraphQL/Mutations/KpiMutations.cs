using HotChocolate.Authorization;
using HotChocolate.Types;
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
        KpiTierDto input,
        [Service] IKpiService kpiService,
 CancellationToken ct)
    {
  try
   {
var result = await kpiService.CreateKpiTierAsync(input, ct);
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
     KpiTierDto input,
        [Service] IKpiService kpiService,
        CancellationToken ct)
  {
 try
 {
            var result = await kpiService.UpdateKpiTierAsync(kpiTierId, input, ct);
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
  SetMonthlyTargetDto input,
   [Service] IKpiService kpiService,
        CancellationToken ct)
    {
 try
     {
    var result = await kpiService.SetMonthlyTargetAsync(input, ct);
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

    // ===== CALCULATE MONTHLY KPI (Admin only, ch?y cu?i tháng) =====
    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
  public async Task<KpiCalculateResultDto> CalculateMonthlyKpi(
        CalculateMonthlyKpiDto input,
    [Service] IKpiService kpiService,
        CancellationToken ct)
    {
   try
    {
      var results = await kpiService.CalculateMonthlyKpiAsync(input, ct);
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
