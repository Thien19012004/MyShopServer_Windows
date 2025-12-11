namespace MyShopServer.Application.GraphQL.Inputs.Orders;

public record OrderItemInput(
    int ProductId,
    int Quantity
);
