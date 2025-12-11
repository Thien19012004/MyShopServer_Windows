namespace MyShopServer.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int ImportPrice { get; set; }
    public int SalePrice { get; set; }
    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
}
