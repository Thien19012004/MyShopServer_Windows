using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Promotions;

public class CreatePromotionInput
{
    public string Name { get; set; } = string.Empty;
    public int DiscountPercent { get; set; } // 0..100
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public PromotionScope Scope { get; set; }

    // For Product scope
    public List<int>? ProductIds { get; set; }
    
    // For Category scope
    public List<int>? CategoryIds { get; set; }
}
