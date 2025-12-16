using HotChocolate.Authorization;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Promotions;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Promotions;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class PromotionQueries
{
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<PromotionListResultDto> Promotions(
        PaginationInput? pagination,
        PromotionFilterInput? filter,
        [Service] IPromotionService promotionService,
        CancellationToken ct)
    {
        try
        {
            var opt = new PromotionQueryOptions
            {
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize ?? 10,
                Search = filter?.Search,
                OnlyActive = filter?.OnlyActive ?? false,
                At = filter?.At
            };

            var paged = await promotionService.GetPromotionsAsync(opt, ct);

            return new PromotionListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get promotions success",
                Data = paged
            };
        }
        catch (Exception ex)
        {
            return new PromotionListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<PromotionResultDto> PromotionById(
        int promotionId,
        [Service] IPromotionService promotionService,
        CancellationToken ct)
    {
        try
        {
            var p = await promotionService.GetPromotionByIdAsync(promotionId, ct);
            if (p == null)
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
                Message = "Get promotion success",
                Data = p
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

    // optional: check best discount of a product at now
    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<IntResultDto> BestDiscountPct(
        int productId,
        DateTime? at,
        [Service] IPromotionService promotionService,
        CancellationToken ct)
    {
        try
        {
            var best = await promotionService.GetBestDiscountPercentAsync(productId, at ?? DateTime.UtcNow, ct);
            return new IntResultDto { StatusCode = 200, Success = true, Message = "OK", Data = best };
        }
        catch (Exception ex)
        {
            return new IntResultDto { StatusCode = 400, Success = false, Message = ex.Message, Data = 0 };
        }
    }
}
