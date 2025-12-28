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

    // Pricing breakdown
    public int Subtotal { get; set; }
    public int OrderDiscountAmount { get; set; }
}
