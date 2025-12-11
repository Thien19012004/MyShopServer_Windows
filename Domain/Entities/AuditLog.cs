namespace MyShopServer.Domain.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
