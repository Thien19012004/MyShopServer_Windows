using MyShopServer.Domain.Enums;

namespace MyShopServer.Domain.Entities
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        public string Name { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public PromotionScope Scope { get; set; }

        public ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
        public ICollection<CategoryPromotion> CategoryPromotions { get; set; } = new List<CategoryPromotion>();
        public ICollection<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();
    
    }
}
