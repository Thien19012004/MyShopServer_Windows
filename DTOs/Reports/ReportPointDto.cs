namespace MyShopServer.DTOs.Reports;

public class ReportPointDto
{
    public DateTime Period { get; set; }   // start date of bucket (day/week/month/year)
    public int Value { get; set; }
}
