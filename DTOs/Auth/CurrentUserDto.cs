namespace MyShopServer.DTOs.Auth;

public class CurrentUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string[] Roles { get; set; } = Array.Empty<string>();
}
