using MyShopServer.Domain.Enums;

namespace MyShopServer.DTOs.Reports;

public class RecentOrderDto
{
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? SaleName { get; set; }
    public OrderStatus Status { get; set; }
    public int TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
