namespace MyShopServer.DTOs.Products;

public class ProductDetailDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int ImportPrice { get; set; }
    public int SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public List<string> ImagePaths { get; set; } = new();
}
