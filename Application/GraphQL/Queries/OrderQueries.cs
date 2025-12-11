using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Orders;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Orders;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class OrderQueries
{
    public async Task<OrderListResultDto> Orders(
        PaginationInput? pagination,
        OrderFilterInput? filter,
        DateRangeInput? dateRange,
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

    public async Task<OrderResultDto> OrderById(
        int orderId,
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
