using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Users;

public class UpdateUserInput
{
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>
    /// Optional: if provided => replace roles.
    /// </summary>
    public List<RoleName>? Roles { get; set; }
}
