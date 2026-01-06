using MyShopServer.Domain.Enums;

namespace MyShopServer.DTOs.Users;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<RoleName> Roles { get; set; } = new();
}

public class CreateUserDto
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

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>
    /// Optional: if null => keep roles unchanged; if provided => replace roles.
    /// </summary>
    public List<RoleName>? Roles { get; set; }
}

public class UserQueryOptions
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search by username/fullName (accent-insensitive)
    /// </summary>
    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter: only users having this role.
    /// </summary>
    public RoleName? Role { get; set; }
}
