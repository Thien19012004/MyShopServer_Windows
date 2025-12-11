using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Auth;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Auth;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AuthMutations
{
    public async Task<LoginResultDto> Login(
        LoginInput input,
        [Service] IAuthService authService)
    {
        try
        {
            var dto = await authService.LoginAsync(input.Username, input.Password);

            return new LoginResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Login success",
                Data = dto
            };
        }
        catch
        {
            return new LoginResultDto
            {
                StatusCode = 401,
                Success = false,
                Message = "Invalid username or password",
                Data = null
            };
        }
    }
}
