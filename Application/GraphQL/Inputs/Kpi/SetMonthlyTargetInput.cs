namespace MyShopServer.Application.GraphQL.Inputs.Kpi;

public class SetMonthlyTargetInput
{
    public int SaleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TargetRevenue { get; set; }
}
