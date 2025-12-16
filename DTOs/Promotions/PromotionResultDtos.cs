using MyShopServer.DTOs.Common;

namespace MyShopServer.DTOs.Promotions;

public class PromotionResultDto : MutationResult<PromotionDto> { }
public class PromotionListResultDto : MutationResult<PagedResult<PromotionDto>> { }
public class IntResultDto : MutationResult<int> { }   // dùng cho bestDiscountPct
