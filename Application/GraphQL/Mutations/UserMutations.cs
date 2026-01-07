using HotChocolate.Authorization;
using MyShopServer.Application.GraphQL.Inputs.Users;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Users;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UserMutations
{
    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
    public async Task<UserResultDto> CreateUser(
    CreateUserInput input,
    [Service] IUserService userService,
    CancellationToken ct)
    {
        try
        {
            var dto = new CreateUserDto
            {
                Username = input.Username,
                Password = input.Password,
                FullName = input.FullName,
                IsActive = input.IsActive,
                Roles = input.Roles
            };

            var created = await userService.CreateUserAsync(dto, ct);

            return new UserResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "User created successfully",
                Data = created
            };
        }
        catch (Exception ex)
        {
            return new UserResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<UserResultDto> UpdateUser(
    int userId,
    UpdateUserInput input,
    [Service] IUserService userService,
    CancellationToken ct)
    {
        try
        {
            var dto = new UpdateUserDto
            {
                FullName = input.FullName,
                IsActive = input.IsActive,
                Roles = input.Roles
            };

            var updated = await userService.UpdateUserAsync(userId, dto, ct);
            if (updated == null)
            {
                return new UserResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "User not found",
                    Data = null
                };
            }

            return new UserResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "User updated successfully",
                Data = updated
            };
        }
        catch (Exception ex)
        {
            return new UserResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<UserBoolResultDto> SetUserActive(
    int userId,
    bool isActive,
    [Service] IUserService userService,
    CancellationToken ct)
    {
        try
        {
            var ok = await userService.SetUserActiveAsync(userId, isActive, ct);
            if (!ok)
            {
                return new UserBoolResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "User not found",
                    Data = false
                };
            }

            return new UserBoolResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "OK",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new UserBoolResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = false
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin) })]
    public async Task<UserBoolResultDto> ResetUserPassword(
    ResetPasswordInput input,
    [Service] IUserService userService,
    CancellationToken ct)
    {
        try
        {
            var ok = await userService.ResetPasswordAsync(input.UserId, input.NewPassword, ct);
            if (!ok)
            {
                return new UserBoolResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "User not found",
                    Data = false
                };
            }

            return new UserBoolResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Password reset successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new UserBoolResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = false
            };
        }
    }
}
