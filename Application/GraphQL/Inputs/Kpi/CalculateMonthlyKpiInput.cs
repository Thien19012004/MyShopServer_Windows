namespace MyShopServer.Application.GraphQL.Inputs.Kpi;

public class CalculateMonthlyKpiInput
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int? SaleId { get; set; }
}
