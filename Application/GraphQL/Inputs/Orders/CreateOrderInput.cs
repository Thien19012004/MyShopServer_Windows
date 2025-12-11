namespace MyShopServer.Application.GraphQL.Inputs.Orders;

public record CreateOrderInput(
    int? CustomerId,
    int SaleId,
    List<OrderItemInput> Items
);
