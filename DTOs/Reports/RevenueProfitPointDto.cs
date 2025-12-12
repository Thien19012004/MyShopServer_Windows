namespace MyShopServer.DTOs.Reports;

public class RevenueProfitPointDto
{
    public DateTime Period { get; set; }
    public int Revenue { get; set; }
    public int Profit { get; set; }
}
