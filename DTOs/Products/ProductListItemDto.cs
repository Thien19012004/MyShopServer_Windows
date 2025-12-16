namespace MyShopServer.DTOs.Products;

public class ProductListItemDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int SalePrice { get; set; }
    public int ImportPrice { get; set; }   // sau có thể ẩn với Sale
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = default!;
    public int DiscountPct { get; set; }     
    public int FinalPrice { get; set; }
}
