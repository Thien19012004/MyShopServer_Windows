namespace MyShopServer.DTOs.Kpi;

public class KpiTierDto
{
    public int KpiTierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinRevenue { get; set; }
  public int BonusPercent { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class SaleKpiTargetDto
{
    public int SaleKpiTargetId { get; set; }
    public int SaleId { get; set; }
    public string? SaleName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TargetRevenue { get; set; }
    public int ActualRevenue { get; set; }
    public int? KpiTierId { get; set; }
    public string? KpiTierName { get; set; }
    public int BonusAmount { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
  
    // Computed
    public int Progress => TargetRevenue > 0 ? (ActualRevenue * 100 / TargetRevenue) : 0;
}

public class KpiCommissionDto
{
    public int KpiCommissionId { get; set; }
    public int SaleId { get; set; }
    public string? SaleName { get; set; }
 public int Year { get; set; }
    public int Month { get; set; }
    public int BaseCommission { get; set; }
    public int BonusCommission { get; set; }
    public int TotalCommission { get; set; }
    public int? KpiTierId { get; set; }
    public string? KpiTierName { get; set; }
    public int TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class KpiDashboardDto
{
    public int SaleId { get; set; }
    public string SaleName { get; set; } = string.Empty;
    public int CurrentYear { get; set; }
    public int CurrentMonth { get; set; }
    
    // Current month progress
    public int TargetRevenue { get; set; }
    public int ActualRevenue { get; set; }
    public int Progress => TargetRevenue > 0 ? (ActualRevenue * 100 / TargetRevenue) : 0;
    public int RemainingRevenue => Math.Max(0, TargetRevenue - ActualRevenue);
    
    // Commission preview (current month, ch?a finalized)
    public int EstimatedBaseCommission { get; set; }
    public int EstimatedBonusCommission { get; set; }
    public int EstimatedTotalCommission { get; set; }
    
 // Current tier (d? ki?n)
    public int? CurrentKpiTierId { get; set; }
    public string? CurrentKpiTierName { get; set; }
    
    // Stats
    public int TotalOrdersThisMonth { get; set; }
  public int TotalOrdersPaid { get; set; }
    
    // KPI Tiers available
 public List<KpiTierDto> AvailableTiers { get; set; } = new();
}

public class SetMonthlyTargetDto
{
    public int SaleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TargetRevenue { get; set; }
}

public class CalculateMonthlyKpiDto
{
    public int Year { get; set; }
    public int Month { get; set; }
  public int? SaleId { get; set; } // null = tính cho t?t c? sales
}
