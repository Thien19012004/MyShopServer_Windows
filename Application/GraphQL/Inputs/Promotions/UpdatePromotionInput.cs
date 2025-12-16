namespace MyShopServer.Application.GraphQL.Inputs.Promotions;

public class UpdatePromotionInput
{
    public string? Name { get; set; }
    public int? DiscountPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // nếu truyền => replace toàn bộ list sản phẩm áp dụng
    public List<int>? ProductIds { get; set; }
}
