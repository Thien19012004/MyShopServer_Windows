namespace MyShopServer.Domain.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
