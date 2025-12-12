namespace MyShopServer.Application.GraphQL.Inputs.Reports;

public record ProductSalesReportFilterInput(
    int? CategoryId,
    int Top = 10
);
