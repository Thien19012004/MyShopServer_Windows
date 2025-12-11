using MyShopServer.Domain.Enums;

namespace MyShopServer.Application.GraphQL.Inputs.Orders;

public record OrderFilterInput(
    int? CustomerId,
    int? SaleId,
    OrderStatus? Status
);
