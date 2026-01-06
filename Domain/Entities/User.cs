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
  
        // KPI
        public ICollection<SaleKpiTarget> SaleKpiTargets { get; set; } = new List<SaleKpiTarget>();
        public ICollection<KpiCommission> KpiCommissions { get; set; } = new List<KpiCommission>();

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
