namespace MyShopServer.Domain.Entities;

/// <summary>
/// Hoa hồng KPI của sale (base + bonus)
/// </summary>
public class KpiCommission
{
    public int KpiCommissionId { get; set; }

    public int SaleId { get; set; }
    public User Sale { get; set; } = null!;

    public int Year { get; set; }
    public int Month { get; set; }

    public int BaseCommission { get; set; } // Hoa hồng cơ bản (mặc định 10%) cho các đơn hàng Paid trong tháng.


    public int BonusCommission { get; set; } // Hoa hồng thưởng (theo tier đạt được trong tháng).


    public int TotalCommission { get; set; } // Tổng hoa hồng (Base + Bonus) trong tháng.

    public int? KpiTierId { get; set; } // Tier đạt được trong tháng (null nếu không có target hoặc chưa đạt target).
    public KpiTier? KpiTier { get; set; }


    public int TotalRevenue { get; set; } // Tổng doanh số tháng (từ các đơn PAID trong tháng).


    public int TotalOrders { get; set; } // Tổng số đơn hàng Paid trong tháng.

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
