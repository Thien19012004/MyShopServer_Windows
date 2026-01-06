namespace MyShopServer.Application.GraphQL.Inputs.Kpi;

public class UpdateKpiTierInput
{
    public string? Name { get; set; }
    public int? MinRevenue { get; set; }
    public int? BonusPercent { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}
