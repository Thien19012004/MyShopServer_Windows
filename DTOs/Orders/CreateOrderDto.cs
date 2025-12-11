namespace MyShopServer.DTOs.Orders;

public class CreateOrderDto
{
    public int? CustomerId { get; set; }
    public int SaleId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
