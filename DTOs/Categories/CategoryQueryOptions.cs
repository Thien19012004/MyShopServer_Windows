namespace MyShopServer.DTOs.Categories;

public class CategoryQueryOptions
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // optional: lọc theo tên
    public string? Search { get; set; }
}
