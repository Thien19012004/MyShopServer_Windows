using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Promotions;

public class PromotionFilterInput
{
    public string? Search { get; set; }
    public bool OnlyActive { get; set; } = false;

    // thời điểm để check active; null => now
    public DateTime? At { get; set; }
    
    // Filter by promotion scope (PRODUCT, CATEGORY, ORDER)
    public PromotionScope? Scope { get; set; }
}
