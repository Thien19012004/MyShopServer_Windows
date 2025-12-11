
using MyShopServer.Domain.Enums;

namespace MyShopServer.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public PaymentMethod Method { get; set; }
    public int Amount { get; set; }

    public DateTime PaidAt { get; set; } = DateTime.Now;
}
