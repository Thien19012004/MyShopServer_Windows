using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Orders;

namespace MyShopServer.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDetailDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<OrderDetailDto> UpdateOrderAsync(int orderId, UpdateOrderDto dto, CancellationToken ct = default);
    Task<bool> DeleteOrderAsync(int orderId, CancellationToken ct = default);

    Task<OrderDetailDto?> GetOrderByIdAsync(int orderId, CancellationToken ct = default);
    Task<PagedResult<OrderListItemDto>> GetOrdersAsync(OrderQueryOptions options, CancellationToken ct = default);
}
