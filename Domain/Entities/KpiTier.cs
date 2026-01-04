namespace MyShopServer.Domain.Entities;


// KPI Tier (Bronze, Silver, Gold, Platinum) với phần trăm hoa hồng thưởng (bonus commission %).
public class KpiTier
{
    public int KpiTierId { get; set; }

    public string Name { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum

    //Ngưỡng tối thiểu để đạt tier.
    //Lưu ý: theo logic KPI đơn giản hiện tại, giá trị này đang được dùng như
    //"% hoàn thành target" (MinAchievedPercent), ví dụ:100/120/150/200.
    public int MinRevenue { get; set; }

    public int BonusPercent { get; set; } //  % bonus thêm (tính trên tổng doanh số tháng).

    public string? Description { get; set; }

    public int DisplayOrder { get; set; } // Thứ tự hiển thị (1=Bronze,2=Silver, ...)

    // Navigation
    public ICollection<SaleKpiTarget> SaleKpiTargets { get; set; } = new List<SaleKpiTarget>();
    public ICollection<KpiCommission> KpiCommissions { get; set; } = new List<KpiCommission>();
}
