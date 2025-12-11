using MyShopServer.DTOs.Products;

namespace MyShopServer.Application.GraphQL.Inputs.Products;

public record ProductSortInput(
    ProductSortBy Field,
    bool Asc = true
);
