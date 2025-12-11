namespace MyShopServer.DTOs.Reports;

public class DashboardOverviewDto
{
    public int TotalProducts { get; set; }
    public int TotalOrdersToday { get; set; }
    public int RevenueToday { get; set; }
}
