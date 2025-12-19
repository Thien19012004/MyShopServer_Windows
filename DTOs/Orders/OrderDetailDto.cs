using MyShopServer.Domain.Enums;

namespace MyShopServer.DTOs.Orders;

public class OrderDetailDto
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public int SaleId { get; set; }
    public string? SaleName { get; set; }

    public OrderStatus Status { get; set; }
    public int TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
    public List<int> PromotionIds { get; set; } = new();

    // New: pricing breakdown
    public int Subtotal { get; set; }
    public int OrderDiscountAmount { get; set; }
    public int OrderDiscountPercentApplied { get; set; }
}
