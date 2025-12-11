namespace MyShopServer.Domain.Entities
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ImagePath { get; set; } = string.Empty;
    }
}
