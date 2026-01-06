using HotChocolate.Authorization;
using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Users;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Users;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class UserQueries
{
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<UserListResultDto> Users(
    PaginationInput? pagination,
    UserFilterInput? filter,
    [Service] IUserService userService,
    CancellationToken ct)
    {
        try
        {
            var opt = new UserQueryOptions
            {
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize ?? 10,
                Search = filter?.Search,
                IsActive = filter?.IsActive,
                Role = filter?.Role
            };

            var paged = await userService.GetUsersAsync(opt, ct);

            return new UserListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get users success",
                Data = paged
            };
        }
        catch (Exception ex)
        {
            return new UserListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<UserResultDto> UserById(
    int userId,
    [Service] IUserService userService,
    CancellationToken ct)
    {
        try
        {
            var user = await userService.GetUserByIdAsync(userId, ct);
            if (user == null)
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
                Message = "Get user success",
                Data = user
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
}
