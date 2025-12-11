namespace MyShopServer.Application.GraphQL.Inputs.Categories;

public record CreateCategoryInput(
    string Name,
    string? Description
);
