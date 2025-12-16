using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Promotions;

namespace MyShopServer.Application.Services.Interfaces;

public interface IPromotionService
{
    Task<PagedResult<PromotionDto>> GetPromotionsAsync(PromotionQueryOptions options, CancellationToken ct = default);
    Task<PromotionDto?> GetPromotionByIdAsync(int promotionId, CancellationToken ct = default);

    Task<PromotionDto> CreatePromotionAsync(PromotionDto dto, CancellationToken ct = default);
    Task<PromotionDto?> UpdatePromotionAsync(int promotionId, PromotionDto dto, bool replaceProducts, CancellationToken ct = default);
    Task<bool> DeletePromotionAsync(int promotionId, CancellationToken ct = default);

    Task<int> GetBestDiscountPercentAsync(int productId, DateTime at, CancellationToken ct = default);
}
