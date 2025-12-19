namespace MyShopServer.Domain.Entities;

public class CategoryPromotion
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int PromotionId { get; set; }
    public Promotion Promotion { get; set; } = default!;
}
