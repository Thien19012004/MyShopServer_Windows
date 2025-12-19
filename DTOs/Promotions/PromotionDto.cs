using MyShopServer.Domain.Enums;

namespace MyShopServer.DTOs.Promotions;

public class PromotionDto
{
    public int PromotionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
 
    public PromotionScope Scope { get; set; }

    // For Product scope
    public int ProductCount { get; set; }
    public List<int> ProductIds { get; set; } = new();
    
    // For Category scope
    public int CategoryCount { get; set; }
    public List<int> CategoryIds { get; set; } = new();
}
