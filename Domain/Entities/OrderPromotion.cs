namespace MyShopServer.Domain.Entities;

public class OrderPromotion
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public int PromotionId { get; set; }
    public Promotion Promotion { get; set; } = default!;
}