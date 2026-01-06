namespace MyShopServer.Application.GraphQL.Inputs.Kpi;

public class SaleKpiTargetFilterInput
{
    public int? SaleId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
}
