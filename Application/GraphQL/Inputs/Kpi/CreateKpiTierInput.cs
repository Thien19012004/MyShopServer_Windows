namespace MyShopServer.Application.GraphQL.Inputs.Kpi;

public class CreateKpiTierInput
{
    public string Name { get; set; } = string.Empty;
    public int MinRevenue { get; set; }
    public int BonusPercent { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
