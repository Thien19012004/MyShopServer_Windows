namespace MyShopServer.DTOs.Reports;

public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int TotalQuantity { get; set; }
    public int TotalRevenue { get; set; }
}
