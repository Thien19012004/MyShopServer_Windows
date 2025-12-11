namespace MyShopServer.DTOs.Orders;

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
    public int TotalPrice { get; set; }
}
