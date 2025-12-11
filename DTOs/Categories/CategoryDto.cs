namespace MyShopServer.DTOs.Categories;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
}
