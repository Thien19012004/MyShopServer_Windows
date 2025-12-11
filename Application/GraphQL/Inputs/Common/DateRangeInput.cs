namespace MyShopServer.Application.GraphQL.Inputs.Common;

public record DateRangeInput(
    DateTime? From,
    DateTime? To
);
