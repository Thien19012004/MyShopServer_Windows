using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Users;

public class UserFilterInput
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public RoleName? Role { get; set; }
}
