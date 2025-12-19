using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Promotions;

public class UpdatePromotionInput
{
    public string? Name { get; set; }
    public int? DiscountPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public PromotionScope? Scope { get; set; }

    // For Product scope - nếu truyền => replace toàn bộ list sản phẩm áp dụng
    public List<int>? ProductIds { get; set; }
    
    // For Category scope
    public List<int>? CategoryIds { get; set; }
}
