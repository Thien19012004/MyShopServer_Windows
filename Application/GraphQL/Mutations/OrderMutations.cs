using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Orders;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Orders;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class OrderMutations
{
    public async Task<OrderResultDto> CreateOrder(
        CreateOrderInput input,
        [Service] IOrderService orderService,
        CancellationToken ct)
    {
        try
        {
            var dto = new CreateOrderDto
            {
                CustomerId = input.CustomerId,
                SaleId = input.SaleId,
                Items = input.Items
                    .Select(i => new CreateOrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    })
                    .ToList()
            };

            var created = await orderService.CreateOrderAsync(dto, ct);

            return new OrderResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "Order created successfully",
                Data = created
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

    public async Task<OrderResultDto> UpdateOrder(
        int orderId,
        UpdateOrderInput input,
        [Service] IOrderService orderService,
        CancellationToken ct)
    {
        try
        {
            var dto = new UpdateOrderDto
            {
                CustomerId = input.CustomerId,
                Status = input.Status,
                Items = input.Items?.Select(i => new UpdateOrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var updated = await orderService.UpdateOrderAsync(orderId, dto, ct);

            return new OrderResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Order updated successfully",
                Data = updated
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

    public async Task<OrderResultDto> DeleteOrder(
        int orderId,
        [Service] IOrderService orderService,
        CancellationToken ct)
    {
        try
        {
            var ok = await orderService.DeleteOrderAsync(orderId, ct);

            if (!ok)
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
                Message = "Order deleted successfully",
                Data = null
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
