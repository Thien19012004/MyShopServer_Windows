using MyShopServer.DTOs.Common;

namespace MyShopServer.DTOs.Reports;

public class DashboardOverviewResultDto : MutationResult<DashboardOverviewDto> { }

public class LowStockProductListResultDto : MutationResult<List<LowStockProductDto>> { }

public class TopSellingProductListResultDto : MutationResult<List<TopSellingProductDto>> { }

public class RecentOrderListResultDto : MutationResult<List<RecentOrderDto>> { }

public class DailyRevenueListResultDto : MutationResult<List<DailyRevenuePointDto>> { }
