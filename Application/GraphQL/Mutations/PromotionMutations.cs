using HotChocolate.Authorization;
using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Promotions;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Promotions;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class PromotionMutations
{
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<PromotionResultDto> CreatePromotion(
        CreatePromotionInput input,
        [Service] IPromotionService promotionService,
        CancellationToken ct)
    {
        try
        {
            var dto = new PromotionDto
            {
                Name = input.Name,
                DiscountPercent = input.DiscountPercent,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Scope = input.Scope,
                ProductIds = input.ProductIds ?? new(),
                CategoryIds = input.CategoryIds ?? new()
            };

            var created = await promotionService.CreatePromotionAsync(dto, ct);

            return new PromotionResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "Promotion created successfully",
                Data = created
            };
        }
        catch (Exception ex)
        {
            return new PromotionResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<PromotionResultDto> UpdatePromotion(
        int promotionId,
        UpdatePromotionInput input,
        [Service] IPromotionService promotionService,
        CancellationToken ct)
    {
        try
        {
            var existing = await promotionService.GetPromotionByIdAsync(promotionId, ct);
            if (existing == null)
            {
                return new PromotionResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Promotion not found",
                    Data = null
                };
            }

            var replaceProducts = input.ProductIds != null;

            var dto = new PromotionDto
            {
                PromotionId = promotionId,
                Name = input.Name ?? existing.Name,
                DiscountPercent = input.DiscountPercent ?? existing.DiscountPercent,
                StartDate = input.StartDate ?? existing.StartDate,
                EndDate = input.EndDate ?? existing.EndDate,
                Scope = input.Scope ?? existing.Scope,
                ProductIds = input.ProductIds ?? existing.ProductIds,
                CategoryIds = input.CategoryIds ?? existing.CategoryIds
            };

            var updated = await promotionService.UpdatePromotionAsync(promotionId, dto, replaceProducts, ct);

            return new PromotionResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Promotion updated successfully",
                Data = updated
            };
        }
        catch (Exception ex)
        {
            return new PromotionResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<PromotionResultDto> DeletePromotion(
        int promotionId,
        [Service] IPromotionService promotionService,
        CancellationToken ct)
    {
        try
        {
            var ok = await promotionService.DeletePromotionAsync(promotionId, ct);
            if (!ok)
            {
                return new PromotionResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Promotion not found",
                    Data = null
                };
            }

            return new PromotionResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Promotion deleted successfully",
                Data = null
            };
        }
        catch (Exception ex)
        {
            return new PromotionResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
