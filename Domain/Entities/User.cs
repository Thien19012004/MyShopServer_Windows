namespace MyShopServer.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<Order> Orders { get; set; }      // Sale tạo đơn
        public ICollection<Commission> Commissions { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; }
    }
}
