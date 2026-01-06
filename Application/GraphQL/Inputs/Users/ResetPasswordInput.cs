namespace MyShopServer.Application.GraphQL.Inputs.Users;

public class ResetPasswordInput
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
