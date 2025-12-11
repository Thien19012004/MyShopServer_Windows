using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Orders;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Orders;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class OrderQueries
{
    // Chỉ cho Admin/Moderator/Sale xem danh sách
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator),
        nameof(RoleName.Sale)
    })]
    public async Task<OrderListResultDto> Orders(
        PaginationInput? pagination,
        OrderFilterInput? filter,
        DateRangeInput? dateRange,
        ClaimsPrincipal currentUser,
        [Service] IOrderService orderService,
        CancellationToken ct)
    {
        try
        {
            var options = new OrderQueryOptions
            {
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize ?? 10,
                CustomerId = filter?.CustomerId,
                SaleId = filter?.SaleId,
                Status = filter?.Status,
                FromDate = dateRange?.From,
                ToDate = dateRange?.To
            };

            // Nếu là SALE → chỉ xem đơn của chính mình
            if (currentUser.IsInRole(nameof(RoleName.Sale)))
            {
                var idStr = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out var saleId))
                {
                    options.SaleId = saleId; // override filter client
                }
            }

            var paged = await orderService.GetOrdersAsync(options, ct);

            return new OrderListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get orders success",
                Data = paged
            };
        }
        catch (Exception ex)
        {
            return new OrderListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    // Xem chi tiết 1 đơn hàng
    [Authorize(Roles = new[]
    {
        nameof(RoleName.Admin),
        nameof(RoleName.Moderator),
        nameof(RoleName.Sale)
    })]
    public async Task<OrderResultDto> OrderById(
        int orderId,
        ClaimsPrincipal currentUser,
        [Service] IOrderService orderService,
        CancellationToken ct)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(orderId, ct);

            if (order == null)
            {
                return new OrderResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Order not found",
                    Data = null
                };
            }

            // Nếu là SALE → chỉ xem được đơn của mình
            if (currentUser.IsInRole(nameof(RoleName.Sale)))
            {
                var idStr = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out var saleId) && order.SaleId != saleId)
                {
                    return new OrderResultDto
                    {
                        StatusCode = 403,
                        Success = false,
                        Message = "You are not allowed to view this order.",
                        Data = null
                    };
                }
            }

            return new OrderResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get order success",
                Data = order
            };
        }
        catch (Exception ex)
        {
            return new OrderResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}

