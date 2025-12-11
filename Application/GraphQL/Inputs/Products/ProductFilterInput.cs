namespace MyShopServer.Application.GraphQL.Inputs.Products;

public record ProductFilterInput(
    int? CategoryId,
    string? Search,
    int? MinPrice,
    int? MaxPrice
);
