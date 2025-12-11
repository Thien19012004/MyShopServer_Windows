using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Orders;

public record UpdateOrderInput(
    int? CustomerId,
    OrderStatus? Status,
    List<OrderItemInput>? Items
);
