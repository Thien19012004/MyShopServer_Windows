using MyShopServer.Domain.Enums;

namespace MyShopServer.DTOs.Orders;

public class UpdateOrderDto
{
    public int? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    // nếu null => giữ nguyên items, nếu có => replace toàn bộ items
    public List<UpdateOrderItemDto>? Items { get; set; }
    public List<int>? PromotionIds { get; set; }
}

public class UpdateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
