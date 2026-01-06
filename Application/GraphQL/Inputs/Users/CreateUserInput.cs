using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Users;

public class CreateUserInput
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Required. Example: [Sale]
    /// </summary>
    public List<RoleName> Roles { get; set; } = new();
}
