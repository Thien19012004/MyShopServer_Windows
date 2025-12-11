using MyShopServer.Domain.Enums;

namespace MyShopServer.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public RoleName RoleName { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
