namespace MyShopServer.DTOs.Reports;

// line chart
public class ProductSalesSeriesDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // points theo thời gian: Value = quantity sold
    public List<ReportPointDto> Points { get; set; } = new();
}
