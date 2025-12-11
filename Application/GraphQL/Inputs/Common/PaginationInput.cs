namespace MyShopServer.Application.GraphQL.Inputs.Common;

public record PaginationInput(
    int Page = 1,
    int PageSize = 10
);
