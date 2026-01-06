using MyShopServer.DTOs.Common;

namespace MyShopServer.DTOs.Kpi;

public class KpiTierResultDto : MutationResult<KpiTierDto> { }
public class KpiTierListResultDto : MutationResult<PagedResult<KpiTierDto>> { }

public class SaleKpiTargetResultDto : MutationResult<SaleKpiTargetDto> { }
public class SaleKpiTargetListResultDto : MutationResult<PagedResult<SaleKpiTargetDto>> { }

public class KpiCommissionResultDto : MutationResult<KpiCommissionDto> { }
public class KpiCommissionListResultDto : MutationResult<PagedResult<KpiCommissionDto>> { }

public class KpiDashboardResultDto : MutationResult<KpiDashboardDto> { }
public class KpiCalculateResultDto : MutationResult<List<KpiCommissionDto>> { }
