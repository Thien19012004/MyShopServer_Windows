using MyShopServer.Domain.Enums;

namespace MyShopServer.DTOs.Orders;

public class OrderListItemDto
{
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? SaleName { get; set; }
    public OrderStatus Status { get; set; }
    public int TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }

    // New: pricing breakdown (best-effort)
    public int OrderDiscountAmount { get; set; }
}
