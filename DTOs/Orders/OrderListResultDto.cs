using MyShopServer.DTOs.Common;

namespace MyShopServer.DTOs.Orders;

public class OrderListResultDto : MutationResult<PagedResult<OrderListItemDto>>
{
}
