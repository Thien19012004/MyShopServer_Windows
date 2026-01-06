namespace MyShopServer.Domain.Entities;


// Target KPI cho từng Sale theo tháng.
public class SaleKpiTarget
{
    public int SaleKpiTargetId { get; set; }

    public int SaleId { get; set; }
    public User Sale { get; set; } = null!;

    public int Year { get; set; }


    public int Month { get; set; } // Tháng (1-12).

    public int TargetRevenue { get; set; } // Mục tiêu doanh số tháng.

    public int ActualRevenue { get; set; } // Doanh số thực tế 

    public int? KpiTierId { get; set; } // Tier đạt được (null nếu chưa tính hoặc chưa đạt target).
    public KpiTier? KpiTier { get; set; }

    public int BonusAmount { get; set; } // Tiền thưởng KPI (0 nếu chưa đạt).

    public DateTime? CalculatedAt { get; set; } // Thời điểm tính KPI (khi admin chạy calculateMonthlyKpi).

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
