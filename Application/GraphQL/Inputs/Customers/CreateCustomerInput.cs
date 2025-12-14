namespace MyShopServer.Application.GraphQL.Inputs.Customers;

public class CreateCustomerInput
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
}
