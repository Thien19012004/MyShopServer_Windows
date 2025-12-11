namespace MyShopServer.Application.GraphQL.Inputs.Products;

public record CreateProductInput(
    string Sku,
    string Name,
    int ImportPrice,
    int SalePrice,
    int StockQuantity,
    string? Description,
    int CategoryId,
    List<string>? ImagePaths
);
