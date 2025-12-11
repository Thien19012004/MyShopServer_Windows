namespace MyShopServer.Domain.Entities
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        public string Name { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ProductPromotion> ProductPromotions { get; set; }
    }
}
