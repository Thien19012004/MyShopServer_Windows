using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Common;
using MyShopServer.Application.GraphQL.Inputs.Customers;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Customers;

namespace MyShopServer.Application.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class CustomerQueries
{
    public async Task<CustomerListResultDto> Customers(
        PaginationInput? pagination,
        CustomerFilterInput? filter,
        [Service] ICustomerService customerService,
        CancellationToken ct)
    {
        try
        {
            var options = new CustomerQueryOptions
            {
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize ?? 10,
                Search = filter?.Search
            };

            var paged = await customerService.GetCustomersAsync(options, ct);

            return new CustomerListResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get customers success",
                Data = paged
            };
        }
        catch (Exception ex)
        {
            return new CustomerListResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<CustomerResultDto> CustomerById(
        int customerId,
        [Service] ICustomerService customerService,
        CancellationToken ct)
    {
        try
        {
            var customer = await customerService.GetByIdAsync(customerId, ct);
            if (customer == null)
            {
                return new CustomerResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Customer not found",
                    Data = null
                };
            }

            return new CustomerResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Get customer success",
                Data = customer
            };
        }
        catch (Exception ex)
        {
            return new CustomerResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
