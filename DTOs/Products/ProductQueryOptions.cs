namespace MyShopServer.DTOs.Products;

public class ProductQueryOptions
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }

    // ==== sort ====
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
    public bool SortAsc { get; set; } = true;
}
