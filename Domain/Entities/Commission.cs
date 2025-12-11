namespace MyShopServer.Domain.Entities
{
    public class Commission
    {
        public int CommissionId { get; set; }

        public int SaleId { get; set; }
        public User Sale { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int Amount { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.Now;
    }
}
