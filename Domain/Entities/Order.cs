using MyShopServer.Domain.Enums;

namespace MyShopServer.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }
    public int SaleId { get; set; }
    public User Sale { get; set; }

    public OrderStatus Status { get; set; }
    public int TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<OrderItem> Items { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public Customer? Customer { get; set; }

}
