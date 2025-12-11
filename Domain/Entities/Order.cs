namespace MyShopServer.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }
    public int SaleId { get; set; }

    public string Status { get; set; } = "Created";
    public int TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<OrderItem> Items { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
