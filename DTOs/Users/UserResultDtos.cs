using MyShopServer.DTOs.Common;

namespace MyShopServer.DTOs.Users;

public class UserResultDto : MutationResult<UserDto>
{
}

public class UserListResultDto : MutationResult<PagedResult<UserDto>>
{
}

public class UserBoolResultDto : MutationResult<bool> { }