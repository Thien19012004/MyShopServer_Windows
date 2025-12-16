namespace MyShopServer.DTOs.Promotions;

public class PromotionDto
{
    public int PromotionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ProductCount { get; set; }
    public List<int> ProductIds { get; set; } = new();
}
