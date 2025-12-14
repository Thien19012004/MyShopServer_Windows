namespace MyShopServer.Application.GraphQL.Inputs.Customers;

public class UpdateCustomerInput
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
