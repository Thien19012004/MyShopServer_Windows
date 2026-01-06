using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Users;

namespace MyShopServer.Application.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(UserQueryOptions options, CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken ct = default);
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken ct = default);

    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft delete: sets IsActive=false.
    /// </summary>
    Task<bool> DeactivateUserAsync(int userId, CancellationToken ct = default);

    Task<bool> SetUserActiveAsync(int userId, bool isActive, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(int userId, string newPassword, CancellationToken ct = default);
}
