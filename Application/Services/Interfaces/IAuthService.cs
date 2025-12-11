using MyShopServer.DTOs.Auth;

namespace MyShopServer.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(string username, string password);
}
