using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Kpi;

namespace MyShopServer.Application.Services.Interfaces;

public interface IKpiService
{
    // ===== KPI TIERS =====
    Task<PagedResult<KpiTierDto>> GetKpiTiersAsync(int page = 1, int pageSize = 100, CancellationToken ct = default);
    Task<KpiTierDto?> GetKpiTierByIdAsync(int kpiTierId, CancellationToken ct = default);
    Task<KpiTierDto> CreateKpiTierAsync(KpiTierDto dto, CancellationToken ct = default);
    Task<KpiTierDto?> UpdateKpiTierAsync(int kpiTierId, KpiTierDto dto, CancellationToken ct = default);
    Task<bool> DeleteKpiTierAsync(int kpiTierId, CancellationToken ct = default);

    // ===== SALE KPI TARGETS =====
    Task<SaleKpiTargetDto?> SetMonthlyTargetAsync(SetMonthlyTargetDto dto, CancellationToken ct = default);
    Task<SaleKpiTargetDto?> GetSaleKpiTargetAsync(int saleId, int year, int month, CancellationToken ct = default);
    Task<PagedResult<SaleKpiTargetDto>> GetSaleKpiTargetsAsync(int? saleId, int? year, int? month, int page = 1, int pageSize = 10, CancellationToken ct = default);

    // ===== KPI COMMISSIONS =====
    Task<List<KpiCommissionDto>> CalculateMonthlyKpiAsync(CalculateMonthlyKpiDto dto, CancellationToken ct = default);
    Task<PagedResult<KpiCommissionDto>> GetKpiCommissionsAsync(int? saleId, int? year, int? month, int page = 1, int pageSize = 10, CancellationToken ct = default);
 Task<KpiCommissionDto?> GetKpiCommissionAsync(int saleId, int year, int month, CancellationToken ct = default);

    // ===== DASHBOARD =====
    Task<KpiDashboardDto> GetKpiDashboardAsync(int saleId, int? year = null, int? month = null, CancellationToken ct = default);
}
