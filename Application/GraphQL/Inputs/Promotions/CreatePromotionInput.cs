namespace MyShopServer.Application.GraphQL.Inputs.Promotions;

public class CreatePromotionInput
{
    public string Name { get; set; } = string.Empty;
    public int DiscountPercent { get; set; } // 0..100
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // gán sản phẩm được áp dụng (optional)
    public List<int>? ProductIds { get; set; }
}
