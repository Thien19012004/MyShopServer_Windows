namespace MyShopServer.Application.GraphQL.Inputs.Common;

public record DateRangeInput(
    [property: GraphQLType(typeof(DateType))] DateTime? From,
    [property: GraphQLType(typeof(DateType))] DateTime? To
);
