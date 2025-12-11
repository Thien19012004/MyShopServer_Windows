namespace MyShopServer.Application.GraphQL.Inputs.Products;

// tất cả nullable để update phần nào truyền phần đó
public record UpdateProductInput(
    string? Sku,
    string? Name,
    int? ImportPrice,
    int? SalePrice,
    int? StockQuantity,
    string? Description,
    int? CategoryId,
    List<string>? ImagePaths
);
