namespace MyShopServer.Application.GraphQL.Inputs.Categories;

public record UpdateCategoryInput(
    string? Name,
    string? Description
);
